using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Segments.Commands;
using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanAspire.Application.Features.Segments.Services;

/// <summary>
/// Background service for automatic segment rebuilding
/// </summary>
public class SegmentAutoRebuildService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SegmentAutoRebuildService> _logger;
    private readonly TimeSpan _rebuildInterval;

    public SegmentAutoRebuildService(
        IServiceProvider serviceProvider,
        ILogger<SegmentAutoRebuildService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Default rebuild interval: every 6 hours, configurable via app settings
        var intervalMinutes = configuration.GetValue<int>("Segments:AutoRebuildIntervalMinutes", 360);
        _rebuildInterval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Segment Auto-Rebuild Service started with interval: {Interval}", _rebuildInterval);

        // Wait for application to fully start
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting scheduled segment rebuild process");

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = new RebuildAllSegmentsCommand();

                var results = await mediator.Send(command, stoppingToken);

                var successCount = results.Count(r => r.Success);
                var failureCount = results.Count(r => !r.Success);

                _logger.LogInformation(
                    "Scheduled segment rebuild completed. Success: {SuccessCount}, Failed: {FailureCount}",
                    successCount, failureCount);

                if (failureCount > 0)
                {
                    _logger.LogWarning("Some segments failed during scheduled rebuild: {FailureCount}", failureCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled segment rebuild");
            }

            await Task.Delay(_rebuildInterval, stoppingToken);
        }

        _logger.LogInformation("Segment Auto-Rebuild Service stopped");
    }
}

/// <summary>
/// Service for triggering segment rebuilds based on data changes
/// </summary>
public class SegmentRebuildTriggerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SegmentRebuildTriggerService> _logger;
    private readonly SemaphoreSlim _rebuildSemaphore;

    public SegmentRebuildTriggerService(
        IServiceProvider serviceProvider,
        ILogger<SegmentRebuildTriggerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rebuildSemaphore = new SemaphoreSlim(3, 3); // Max 3 concurrent rebuilds
    }

    /// <summary>
    /// Triggers a rebuild for all segments when data changes
    /// </summary>
    public async Task TriggerRebuildOnDataChangeAsync(OwnerType? ownerType = null, CancellationToken cancellationToken = default)
    {
        if (!await _rebuildSemaphore.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken))
        {
            _logger.LogWarning("Could not acquire semaphore for data change rebuild - another rebuild is in progress");
            return;
        }

        try
        {
            _logger.LogInformation("Triggering segment rebuild due to data changes for OwnerType: {OwnerType}", ownerType);

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Get active segments that need rebuilding
            var segments = await GetSegmentsNeedingRebuildAsync(scope.ServiceProvider, ownerType, cancellationToken);

            if (segments.Any())
            {
                _logger.LogInformation("Found {SegmentCount} segments to rebuild due to data changes", segments.Count);

                var rebuildTasks = segments.Select(async segment =>
                {
                    try
                    {
                        var command = new RebuildSegmentCommand
                        {
                            Id = Guid.Parse(segment.Id),
                            ForceRebuild = true
                        };

                        var result = await mediator.Send(command, cancellationToken);

                        _logger.LogInformation("Successfully rebuilt segment: {SegmentId} ({SegmentName}) with {MemberCount} members",
                            segment.Id, segment.Name, result.TotalMembers);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error rebuilding segment: {SegmentId}", segment.Id);
                    }
                });

                await Task.WhenAll(rebuildTasks);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering rebuild on data change");
        }
        finally
        {
            _rebuildSemaphore.Release();
        }
    }

    /// <summary>
    /// Triggers rebuild for specific segment
    /// </summary>
    public async Task TriggerRebuildAsync(Guid segmentId, bool forceRebuild = false, CancellationToken cancellationToken = default)
    {
        if (!await _rebuildSemaphore.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken))
        {
            _logger.LogWarning("Could not acquire semaphore for segment {SegmentId} rebuild - another rebuild is in progress", segmentId);
            return;
        }

        try
        {
            _logger.LogInformation("Triggering rebuild for segment: {SegmentId}, Force: {ForceRebuild}", segmentId, forceRebuild);

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var command = new RebuildSegmentCommand
            {
                Id = segmentId,
                ForceRebuild = forceRebuild
            };

            var result = await mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully triggered rebuild for segment: {SegmentId} with {MemberCount} members",
                segmentId, result.TotalMembers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering rebuild for segment: {SegmentId}", segmentId);
        }
        finally
        {
            _rebuildSemaphore.Release();
        }
    }

    private static async Task<List<Domain.Entities.Segment>> GetSegmentsNeedingRebuildAsync(
        IServiceProvider serviceProvider,
        OwnerType? ownerType,
        CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var query = context.Segments
            .Where(s => s.IsActive)
            .AsQueryable();

        if (ownerType.HasValue)
        {
            // Filter segments that target the specific owner type
            // This would require parsing the DefinitionJson to check TargetOwnerTypes
            // For now, return all active segments
        }

        return await query.ToListAsync(cancellationToken);
    }
}
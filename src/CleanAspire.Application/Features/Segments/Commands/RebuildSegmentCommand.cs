using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Application.Features.Segments.Services;
using CleanAspire.Application.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Segments.Commands;

/// <summary>
/// Command for rebuilding a segment's membership.
/// Triggers the Elsa workflow for segment rebuilding.
/// </summary>
public record RebuildSegmentCommand : IRequest<SegmentStatsDto>, IRequiresValidation
{
    public Guid Id { get; init; }
    public bool ForceRebuild { get; init; } = false;
}

/// <summary>
/// Handler for processing RebuildSegmentCommand.
/// Initiates segment rebuilding through Elsa workflow.
/// </summary>
public class RebuildSegmentCommandHandler : IRequestHandler<RebuildSegmentCommand, SegmentStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RebuildSegmentCommandHandler> _logger;
    private readonly ISegmentRebuilderService _segmentRebuilder;

    public RebuildSegmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<RebuildSegmentCommandHandler> logger,
        ISegmentRebuilderService segmentRebuilder)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
        _segmentRebuilder = segmentRebuilder;
    }

    public async ValueTask<SegmentStatsDto> Handle(RebuildSegmentCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Find segment
        var segment = await _context.Segments
            .Include(s => s.Memberships)
            .FirstOrDefaultAsync(s => s.Id == request.Id.ToString() && s.TenantId == _currentUser.TenantId,
                                cancellationToken);

        if (segment == null)
            throw new InvalidOperationException($"Segment with ID {request.Id} not found");

        if (!segment.IsActive)
            throw new InvalidOperationException("Cannot rebuild an inactive segment");

        
        try
        {
            // Use the segment rebuilder service
            var rebuildResult = await _segmentRebuilder.RebuildSegmentAsync(request.Id, request.ForceRebuild, cancellationToken);

            if (!rebuildResult.Success)
            {
                throw new InvalidOperationException($"Failed to rebuild segment: {rebuildResult.ErrorMessage}");
            }

            // Get updated stats
            var stats = await _segmentRebuilder.GetRebuildStatsAsync(request.Id, cancellationToken);

            // Return updated stats
            return new SegmentStatsDto
            {
                SegmentId = rebuildResult.SegmentId,
                SegmentName = rebuildResult.SegmentName,
                TotalMembers = rebuildResult.NewMemberCount,
                ClientMembers = rebuildResult.NewMemberCount, // TODO: Calculate actual split
                ContactMembers = 0, // TODO: Calculate actual split
                LastRebuiltAt = rebuildResult.EndTime,
                RebuildDuration = rebuildResult.Duration,
                RulesCount = 1, // TODO: Count actual rules from definition
                IsActive = true // TODO: Get from segment entity
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding segment {SegmentId}", segment.Id);
            throw new InvalidOperationException($"Failed to rebuild segment: {ex.Message}", ex);
        }
    }
}
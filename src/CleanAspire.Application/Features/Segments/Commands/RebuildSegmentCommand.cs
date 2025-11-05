using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Segments.DTOs;
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

    public RebuildSegmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<RebuildSegmentCommandHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
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

        // Check if rebuild is needed (unless forced)
        if (!request.ForceRebuild && segment.LastRebuiltAt.HasValue &&
            (DateTime.UtcNow - segment.LastRebuiltAt.Value).TotalMinutes < 30)
        {
            _logger.LogInformation("Segment {SegmentId} was rebuilt recently, skipping", segment.Id);
        }

        try
        {
            // TODO: Trigger Elsa workflow here
            // For now, we'll implement a simple rebuild logic

            // Clear existing memberships
            _context.SegmentMemberships.RemoveRange(segment.Memberships);

            // TODO: Implement actual segment evaluation logic here
            // This will be implemented in the Rule Engine step

            // Update segment metadata
            segment.LastRebuiltAt = DateTime.UtcNow;
            segment.MemberCount = 0; // Will be updated after rebuild

            _context.Segments.Update(segment);
            await _context.SaveChangesAsync(cancellationToken);

            // Return updated stats
            return new SegmentStatsDto
            {
                SegmentId = Guid.Parse(segment.Id),
                SegmentName = segment.Name,
                TotalMembers = 0,
                ClientMembers = 0,
                ContactMembers = 0,
                LastRebuiltAt = segment.LastRebuiltAt,
                RebuildDuration = TimeSpan.Zero,
                RulesCount = 1, // TODO: Count actual rules
                IsActive = segment.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding segment {SegmentId}", segment.Id);
            throw new InvalidOperationException($"Failed to rebuild segment: {ex.Message}", ex);
        }
    }
}
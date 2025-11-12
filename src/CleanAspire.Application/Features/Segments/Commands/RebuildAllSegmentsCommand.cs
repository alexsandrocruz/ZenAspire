using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Application.Features.Segments.Services;
using CleanAspire.Application.Pipeline;
using Microsoft.Extensions.Logging;

namespace CleanAspire.Application.Features.Segments.Commands;

/// <summary>
/// Command for rebuilding all active segments.
/// Admin-only operation.
/// </summary>
public record RebuildAllSegmentsCommand : IRequest<List<SegmentRebuildResultDto>>, IRequiresValidation
{
}

/// <summary>
/// Handler for processing RebuildAllSegmentsCommand.
/// Rebuilds all active segments using the segment rebuilder service.
/// </summary>
public class RebuildAllSegmentsCommandHandler : IRequestHandler<RebuildAllSegmentsCommand, List<SegmentRebuildResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RebuildAllSegmentsCommandHandler> _logger;
    private readonly ISegmentRebuilderService _segmentRebuilder;

    public RebuildAllSegmentsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<RebuildAllSegmentsCommandHandler> logger,
        ISegmentRebuilderService segmentRebuilder)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
        _segmentRebuilder = segmentRebuilder;
    }

    public async ValueTask<List<SegmentRebuildResultDto>> Handle(RebuildAllSegmentsCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        try
        {
            _logger.LogInformation("Starting rebuild for all active segments for tenant {TenantId}", _currentUser.TenantId);

            // Use the segment rebuilder service to rebuild all segments
            var rebuildResults = await _segmentRebuilder.RebuildAllSegmentsAsync(cancellationToken);

            // Convert to DTOs
            var dtos = rebuildResults.Select(result => new SegmentRebuildResultDto
            {
                SegmentId = result.SegmentId,
                SegmentName = result.SegmentName,
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                Duration = result.Duration,
                StartTime = result.StartTime,
                EndTime = result.EndTime,
                PreviousMemberCount = result.PreviousMemberCount,
                NewMemberCount = result.NewMemberCount,
                MembersAdded = result.MembersAdded,
                MembersRemoved = result.MembersRemoved,
                TotalEntitiesEvaluated = result.TotalEntitiesEvaluated,
                Warnings = result.Warnings
            }).ToList();

            var successCount = dtos.Count(r => r.Success);
            var failureCount = dtos.Count(r => !r.Success);

            _logger.LogInformation(
                "Rebuild all segments completed for tenant {TenantId}: {SuccessCount} successful, {FailureCount} failed",
                _currentUser.TenantId, successCount, failureCount);

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding all segments for tenant {TenantId}", _currentUser.TenantId);
            throw new InvalidOperationException($"Failed to rebuild all segments: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Validator for RebuildAllSegmentsCommand
/// </summary>
public class RebuildAllSegmentsCommandValidator : AbstractValidator<RebuildAllSegmentsCommand>
{
    public RebuildAllSegmentsCommandValidator(ICurrentUserService currentUser)
    {
        RuleFor(x => x)
            .Must(_ => !string.IsNullOrEmpty(currentUser.TenantId))
            .WithMessage("TenantId is required");
    }
}
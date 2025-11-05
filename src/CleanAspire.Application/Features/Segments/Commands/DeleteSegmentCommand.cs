using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Segments.Commands;

/// <summary>
/// Command for deleting an existing segment.
/// </summary>
public record DeleteSegmentCommand : IRequest<bool>, IRequiresValidation
{
    public Guid Id { get; init; }
}

/// <summary>
/// Handler for processing DeleteSegmentCommand.
/// Deletes a segment and all its memberships.
/// </summary>
public class DeleteSegmentCommandHandler : IRequestHandler<DeleteSegmentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteSegmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<bool> Handle(DeleteSegmentCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Find segment
        var segment = await _context.Segments
            .FirstOrDefaultAsync(s => s.Id == request.Id.ToString() && s.TenantId == _currentUser.TenantId,
                                cancellationToken);

        if (segment == null)
            throw new InvalidOperationException($"Segment with ID {request.Id} not found");

        // Remove segment (cascade will delete memberships)
        _context.Segments.Remove(segment);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
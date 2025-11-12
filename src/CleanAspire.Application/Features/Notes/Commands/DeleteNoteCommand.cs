// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Notes.Commands;

/// <summary>
/// Command for deleting a note.
/// </summary>
public record DeleteNoteCommand(string Id) : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public IEnumerable<string>? Tags => new[] { "notes" };
}

/// <summary>
/// Handler for processing DeleteNoteCommand.
/// </summary>
public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteNoteCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var note = await _context.Notes
            .Include(n => n.Attachments)
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Note with Id '{request.Id}' was not found.");

        // TODO: Delete attachments from blob storage (MinIO/Azure Blob)
        // For now, just remove from database (cascade delete will handle it)

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

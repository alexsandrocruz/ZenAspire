// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Notes.Commands;

/// <summary>
/// Command for updating an existing note.
/// </summary>
public record UpdateNoteCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string Id { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string Body { get; init; } = string.Empty;
    public bool IsPinned { get; init; }
    public bool IsPrivate { get; init; }

    public IEnumerable<string>? Tags => new[] { "notes" };
}

/// <summary>
/// Handler for processing UpdateNoteCommand.
/// </summary>
public class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateNoteCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Note with Id '{request.Id}' was not found.");

        note.Title = request.Title;
        note.Body = request.Body;
        note.IsPinned = request.IsPinned;
        note.IsPrivate = request.IsPrivate;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

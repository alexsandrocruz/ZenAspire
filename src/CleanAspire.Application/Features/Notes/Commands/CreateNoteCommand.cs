// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Notes.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Notes.Commands;

/// <summary>
/// Command for creating a new note.
/// </summary>
public record CreateNoteCommand : IFusionCacheRefreshRequest<NoteDto>, IRequiresValidation
{
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string Body { get; init; } = string.Empty;
    public bool IsPinned { get; init; }
    public bool IsPrivate { get; init; }

    public IEnumerable<string>? Tags => new[] { "notes" };
}

/// <summary>
/// Handler for processing CreateNoteCommand.
/// </summary>
public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, NoteDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateNoteCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var note = new Note
        {
            TenantId = _currentUser.TenantId,
            OwnerType = request.OwnerType,
            OwnerId = request.OwnerId,
            Title = request.Title,
            Body = request.Body,
            IsPinned = request.IsPinned,
            IsPrivate = request.IsPrivate
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return new NoteDto
        {
            Id = note.Id,
            OwnerType = note.OwnerType,
            OwnerId = note.OwnerId,
            Title = note.Title,
            Body = note.Body,
            IsPinned = note.IsPinned,
            IsPrivate = note.IsPrivate,
            Created = note.Created,
            CreatedBy = note.CreatedBy,
            LastModified = note.LastModified,
            LastModifiedBy = note.LastModifiedBy
        };
    }
}

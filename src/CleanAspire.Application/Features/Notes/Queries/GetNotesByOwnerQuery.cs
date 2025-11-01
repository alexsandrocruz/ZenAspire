// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Attachments.DTOs;
using CleanAspire.Application.Features.Notes.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Notes.Queries;

/// <summary>
/// Query for getting all notes for a specific entity.
/// </summary>
public record GetNotesByOwnerQuery(OwnerType OwnerType, string OwnerId) : IFusionCacheRequest<IEnumerable<NoteDto>>
{
    public IEnumerable<string>? Tags => new[] { "notes" };
}

/// <summary>
/// Handler for processing GetNotesByOwnerQuery.
/// </summary>
public class GetNotesByOwnerQueryHandler : IRequestHandler<GetNotesByOwnerQuery, IEnumerable<NoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNotesByOwnerQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<IEnumerable<NoteDto>> Handle(GetNotesByOwnerQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var notes = await _context.Notes
            .Where(n => n.OwnerType == request.OwnerType && n.OwnerId == request.OwnerId)
            .Include(n => n.Attachments)
            .OrderByDescending(n => n.IsPinned) // Pinned notes first
            .ThenByDescending(n => n.Created)   // Then by creation date
            .Select(n => new NoteDto
            {
                Id = n.Id,
                OwnerType = n.OwnerType,
                OwnerId = n.OwnerId,
                Title = n.Title,
                Body = n.Body,
                IsPinned = n.IsPinned,
                IsPrivate = n.IsPrivate,
                Created = n.Created,
                CreatedBy = n.CreatedBy,
                LastModified = n.LastModified,
                LastModifiedBy = n.LastModifiedBy,
                Attachments = n.Attachments.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    NoteId = a.NoteId,
                    FileName = a.FileName,
                    Size = a.Size,
                    ContentType = a.ContentType,
                    BlobKey = a.BlobKey,
                    Created = a.Created,
                    CreatedBy = a.CreatedBy
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return notes;
    }
}

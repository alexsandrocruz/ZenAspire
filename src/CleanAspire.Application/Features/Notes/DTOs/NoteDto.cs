// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Attachments.DTOs;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Notes.DTOs;

/// <summary>
/// Data transfer object for Note entity.
/// </summary>
public class NoteDto
{
    public string Id { get; set; } = string.Empty;
    public OwnerType OwnerType { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public ICollection<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
}

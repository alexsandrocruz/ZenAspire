// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Application.Features.Attachments.DTOs;

/// <summary>
/// Data transfer object for Attachment entity.
/// </summary>
public class AttachmentDto
{
    public string Id { get; set; } = string.Empty;
    public string NoteId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string BlobKey { get; set; } = string.Empty;
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
}

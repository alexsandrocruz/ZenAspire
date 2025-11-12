// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Application.Features.Tags.DTOs;

/// <summary>
/// Data transfer object for Tag entity.
/// </summary>
public class TagDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Description { get; set; }
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
}

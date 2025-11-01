// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Tags.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Tags.Commands;

/// <summary>
/// Command for creating a new tag.
/// </summary>
public record CreateTagCommand : IFusionCacheRefreshRequest<TagDto>, IRequiresValidation
{
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
    public string? Description { get; init; }

    public IEnumerable<string>? Tags => new[] { "tags" };
}

/// <summary>
/// Handler for processing CreateTagCommand.
/// </summary>
public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTagCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Check if tag already exists for this tenant
        var existingTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Name == request.Name, cancellationToken);

        if (existingTag != null)
            throw new InvalidOperationException($"Tag '{request.Name}' already exists for this tenant.");

        var tag = new Tag
        {
            TenantId = _currentUser.TenantId,
            Name = request.Name,
            Color = request.Color,
            Description = request.Description
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            Description = tag.Description,
            Created = tag.Created,
            CreatedBy = tag.CreatedBy
        };
    }
}

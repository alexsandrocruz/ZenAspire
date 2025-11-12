// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Tags.DTOs;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Tags.Queries;

/// <summary>
/// Query for getting all tags linked to a specific entity.
/// </summary>
public record GetTagsByOwnerQuery(OwnerType OwnerType, string OwnerId) : IFusionCacheRequest<IEnumerable<TagDto>>
{
    public IEnumerable<string>? Tags => new[] { "tags", "taglinks" };
}

/// <summary>
/// Handler for processing GetTagsByOwnerQuery.
/// </summary>
public class GetTagsByOwnerQueryHandler : IRequestHandler<GetTagsByOwnerQuery, IEnumerable<TagDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTagsByOwnerQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<IEnumerable<TagDto>> Handle(GetTagsByOwnerQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var tags = await _context.TagLinks
            .Where(tl => tl.OwnerType == request.OwnerType && tl.OwnerId == request.OwnerId)
            .Include(tl => tl.Tag)
            .Select(tl => new TagDto
            {
                Id = tl.Tag.Id,
                Name = tl.Tag.Name,
                Color = tl.Tag.Color,
                Description = tl.Tag.Description,
                Created = tl.Tag.Created,
                CreatedBy = tl.Tag.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return tags;
    }
}

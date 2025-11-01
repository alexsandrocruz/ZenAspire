// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Tags.DTOs;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Tags.Queries;

/// <summary>
/// Query for getting tags (with optional search for autocomplete).
/// </summary>
public record GetTagsQuery : IFusionCacheRequest<IEnumerable<TagDto>>
{
    public string? Search { get; init; }

    public IEnumerable<string>? Tags => new[] { "tags" };
}

/// <summary>
/// Handler for processing GetTagsQuery.
/// </summary>
public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, IEnumerable<TagDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTagsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<IEnumerable<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var query = _context.Tags.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => t.Name.Contains(request.Search));
        }

        var tags = await query
            .OrderBy(t => t.Name)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
                Description = t.Description,
                Created = t.Created,
                CreatedBy = t.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return tags;
    }
}

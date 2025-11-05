using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Models;
using CleanAspire.Domain.Enums;
using CleanAspire.Application.Pipeline;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Segments.Commands;

/// <summary>
/// Command for creating a new segment.
/// Encapsulates all data needed to create a segment entity.
/// </summary>
public record CreateSegmentCommand : IRequest<SegmentDto>, IRequiresValidation
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string DefinitionJson { get; init; } = string.Empty;
    public List<OwnerType> TargetOwnerTypes { get; init; } = new();
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Handler for processing CreateSegmentCommand.
/// Creates a new segment entity and saves it to the database.
/// </summary>
public class CreateSegmentCommandHandler : IRequestHandler<CreateSegmentCommand, SegmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateSegmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<SegmentDto> Handle(CreateSegmentCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Check for duplicate segment name within tenant
        var existingSegment = await _context.Segments
            .AnyAsync(s => s.TenantId == _currentUser.TenantId &&
                          s.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase),
                          cancellationToken);

        if (existingSegment)
            throw new InvalidOperationException($"A segment with name '{request.Name}' already exists");

        // Validate JSON definition
        SegmentDefinition? segmentDefinition;
        try
        {
            segmentDefinition = Newtonsoft.Json.JsonConvert.DeserializeObject<SegmentDefinition>(request.DefinitionJson);
            if (segmentDefinition == null)
                throw new ArgumentException("Invalid segment definition JSON");
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid segment definition JSON: {ex.Message}", ex);
        }

        // Create segment entity
        var segment = new Segment
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = _currentUser.TenantId,
            Name = request.Name,
            Description = request.Description,
            DefinitionJson = request.DefinitionJson,
            IsActive = request.IsActive,
            TargetOwnerTypes = request.TargetOwnerTypes.Any()
                ? request.TargetOwnerTypes
                : new List<OwnerType> { OwnerType.Client, OwnerType.Contact }
        };

        // Set audit fields
        segment.Created = DateTime.UtcNow;
        segment.CreatedBy = _currentUser.UserId;

        // Add to database
        _context.Segments.Add(segment);
        await _context.SaveChangesAsync(cancellationToken);

        // Convert to DTO
        return new SegmentDto
        {
            Id = segment.Id.ToString(),
            TenantId = segment.TenantId,
            Name = segment.Name,
            Description = segment.Description,
            DefinitionJson = segment.DefinitionJson,
            IsActive = segment.IsActive,
            LastRebuiltAt = segment.LastRebuiltAt,
            MemberCount = segment.MemberCount,
            TargetOwnerTypes = segment.TargetOwnerTypes.ToList(),
            Created = segment.Created,
            CreatedBy = segment.CreatedBy,
            LastModified = segment.LastModified,
            LastModifiedBy = segment.LastModifiedBy
        };
    }
}
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
/// Command for updating an existing segment.
/// </summary>
public record UpdateSegmentCommand : IRequest<SegmentDto>, IRequiresValidation
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string DefinitionJson { get; init; } = string.Empty;
    public List<OwnerType> TargetOwnerTypes { get; init; } = new();
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Handler for processing UpdateSegmentCommand.
/// Updates an existing segment entity.
/// </summary>
public class UpdateSegmentCommandHandler : IRequestHandler<UpdateSegmentCommand, SegmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateSegmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<SegmentDto> Handle(UpdateSegmentCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Find segment
        var segment = await _context.Segments
            .FirstOrDefaultAsync(s => s.Id == request.Id.ToString() && s.TenantId == _currentUser.TenantId,
                                cancellationToken);

        if (segment == null)
            throw new InvalidOperationException($"Segment with ID {request.Id} not found");

        // Check for duplicate segment name (excluding current segment)
        var duplicateSegment = await _context.Segments
            .AnyAsync(s => s.TenantId == _currentUser.TenantId &&
                          s.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) &&
                          s.Id != request.Id.ToString(),
                          cancellationToken);

        if (duplicateSegment)
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

        // Update segment properties
        segment.Name = request.Name;
        segment.Description = request.Description;
        segment.DefinitionJson = request.DefinitionJson;
        segment.IsActive = request.IsActive;
        segment.TargetOwnerTypes = request.TargetOwnerTypes.Any()
            ? request.TargetOwnerTypes
            : new List<OwnerType> { OwnerType.Client, OwnerType.Contact };

        // Set audit fields
        segment.LastModified = DateTime.UtcNow;
        segment.LastModifiedBy = _currentUser.UserId;

        // Mark as modified
        _context.Segments.Update(segment);
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
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Segments.DTOs;

/// <summary>
/// Response DTO for available segment fields
/// </summary>
public class SegmentFieldsResponseDto
{
    /// <summary>
    /// Available fields for entity types
    /// </summary>
    public Dictionary<string, List<FieldDefinitionDto>> FieldsByEntityType { get; set; } = new();

    /// <summary>
    /// Supported operators
    /// </summary>
    public List<OperatorDefinitionDto> SupportedOperators { get; set; } = new();
}

/// <summary>
/// Field definition for segment rules
/// </summary>
public class FieldDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public List<string> AllowedValues { get; set; } = new();
}

/// <summary>
/// Operator definition for segment rules
/// </summary>
public class OperatorDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SupportedTypes { get; set; } = new();
    public bool RequiresArrayValue { get; set; }
}

/// <summary>
/// Segment validation result DTO
/// </summary>
public class SegmentValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> AvailableFields { get; set; } = new();
}

/// <summary>
/// Segment rebuild result DTO for API responses
/// </summary>
public class SegmentRebuildResultDto
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int PreviousMemberCount { get; set; }
    public int NewMemberCount { get; set; }
    public int MembersAdded { get; set; }
    public int MembersRemoved { get; set; }
    public int TotalEntitiesEvaluated { get; set; }
    public List<string> Warnings { get; set; } = new();
}
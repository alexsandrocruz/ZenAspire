using System.Text.Json.Serialization;

namespace CleanAspire.ClientApp.Models.Segments;

/// <summary>
/// Represents a segment definition with rules
/// </summary>
public class SegmentDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("rules")]
    public SegmentRuleGroup? Rules { get; set; }

    [JsonPropertyName("targetOwnerTypes")]
    public List<string>? TargetOwnerTypes { get; set; }
}

/// <summary>
/// Represents a group of segment rules with logical operators
/// </summary>
public class SegmentRuleGroup
{
    [JsonPropertyName("all")]
    public List<SegmentRuleBase>? All { get; set; }

    [JsonPropertyName("any")]
    public List<SegmentRuleBase>? Any { get; set; }

    [JsonPropertyName("not")]
    public List<SegmentRuleBase>? Not { get; set; }
}

/// <summary>
/// Base class for segment rules
/// </summary>
public abstract class SegmentRuleBase
{
    [JsonPropertyName("type")]
    public virtual string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single condition rule
/// </summary>
public class SegmentCondition : SegmentRuleBase
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    public override string Type => "condition";
}

/// <summary>
/// DTO for field definition in segment rules
/// </summary>
public class FieldDefinitionDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; }

    [JsonPropertyName("allowedValues")]
    public List<string> AllowedValues { get; set; } = new();
}

/// <summary>
/// DTO for operator definition
/// </summary>
public class OperatorDefinitionDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("supportedTypes")]
    public List<string> SupportedTypes { get; set; } = new();

    [JsonPropertyName("requiresArrayValue")]
    public bool RequiresArrayValue { get; set; }
}

/// <summary>
/// DTO for available fields response
/// </summary>
public class SegmentFieldsResponseDto
{
    [JsonPropertyName("fieldsByEntityType")]
    public Dictionary<string, List<FieldDefinitionDto>> FieldsByEntityType { get; set; } = new();

    [JsonPropertyName("supportedOperators")]
    public List<OperatorDefinitionDto> SupportedOperators { get; set; } = new();
}

/// <summary>
/// DTO for segment creation
/// </summary>
public class CreateSegmentCommand
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("definitionJson")]
    public string DefinitionJson { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for segment update
/// </summary>
public class UpdateSegmentCommand
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("definitionJson")]
    public string DefinitionJson { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for segment validation result
/// </summary>
public class SegmentValidationResultDto
{
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();

    [JsonPropertyName("availableFields")]
    public List<string> AvailableFields { get; set; } = new();
}

/// <summary>
/// DTO for segment statistics
/// </summary>
public class SegmentStatsDto
{
    [JsonPropertyName("segmentId")]
    public Guid SegmentId { get; set; }

    [JsonPropertyName("memberCount")]
    public int MemberCount { get; set; }

    [JsonPropertyName("lastRebuiltAt")]
    public DateTime? LastRebuiltAt { get; set; }

    [JsonPropertyName("evaluationDuration")]
    public TimeSpan? EvaluationDuration { get; set; }

    [JsonPropertyName("membersByOwnerType")]
    public Dictionary<string, int> MembersByOwnerType { get; set; } = new();
}

/// <summary>
/// DTO for segment rebuild result
/// </summary>
public class SegmentRebuildResultDto
{
    [JsonPropertyName("segmentId")]
    public Guid SegmentId { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("membersCount")]
    public int MembersCount { get; set; }

    [JsonPropertyName("evaluationDuration")]
    public TimeSpan? EvaluationDuration { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}
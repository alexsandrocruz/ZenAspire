using CleanAspire.Domain.Enums;
using Newtonsoft.Json;

namespace CleanAspire.Domain.Models;

/// <summary>
/// Base class for segment rule definitions
/// </summary>
public abstract class SegmentRuleBase
{
    /// <summary>
    /// Logical operator used to combine conditions
    /// </summary>
    [JsonProperty("operator")]
    public SegmentLogicalOperator? Operator { get; set; }
}

/// <summary>
/// Represents a single condition in a segment rule
/// </summary>
public class SegmentCondition : SegmentRuleBase
{
    /// <summary>
    /// Field name to evaluate (e.g., "ClientType", "CreatedDate")
    /// </summary>
    [JsonProperty("field")]
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Operation to perform (e.g., "eq", "contains")
    /// </summary>
    [JsonProperty("op")]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Value to compare against (can be string, number, boolean, or array)
    /// </summary>
    [JsonProperty("value")]
    public object? Value { get; set; }
}

/// <summary>
/// Represents a logical group of conditions
/// </summary>
public class SegmentRuleGroup : SegmentRuleBase
{
    /// <summary>
    /// List of conditions that all must be true (AND)
    /// </summary>
    [JsonProperty("all")]
    public List<SegmentRuleBase>? All { get; set; }

    /// <summary>
    /// List of conditions where any must be true (OR)
    /// </summary>
    [JsonProperty("any")]
    public List<SegmentRuleBase>? Any { get; set; }

    /// <summary>
    /// List of conditions that must all be false (NOT)
    /// </summary>
    [JsonProperty("not")]
    public List<SegmentRuleBase>? Not { get; set; }
}

/// <summary>
/// Complete segment definition including metadata
/// </summary>
public class SegmentDefinition
{
    /// <summary>
    /// Segment name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Segment description
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Target owner types (optional, defaults to Client)
    /// </summary>
    [JsonProperty("targetOwnerTypes")]
    public List<OwnerType>? TargetOwnerTypes { get; set; }

    /// <summary>
    /// Root rule group
    /// </summary>
    [JsonProperty("rules")]
    public SegmentRuleGroup Rules { get; set; } = new();
}

/// <summary>
/// Specialized conditions for complex segment rules
/// </summary>
public class SegmentSpecialCondition : SegmentCondition
{
    /// <summary>
    /// Special condition type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Additional parameters for special conditions
    /// </summary>
    [JsonProperty("parameters")]
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Pre-defined special condition types
/// </summary>
public static class SegmentSpecialConditionTypes
{
    /// <summary>
    /// Has interaction of specific type
    /// </summary>
    public const string HasInteraction = "hasInteraction";

    /// <summary>
    /// Has activity of specific type
    /// </summary>
    public const string HasActivity = "hasActivity";

    /// <summary>
    /// Is member of another segment
    /// </summary>
    public const string InSegment = "inSegment";

    /// <summary>
    /// Has note containing specific text
    /// </summary>
    public const string HasNote = "hasNote";

    /// <summary>
    /// Has specific consent status
    /// </summary>
    public const string HasConsent = "hasConsent";
}
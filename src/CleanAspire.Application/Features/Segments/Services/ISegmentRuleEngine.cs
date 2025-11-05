using CleanAspire.Domain.Models;

namespace CleanAspire.Application.Features.Segments.Services;

/// <summary>
/// Service interface for evaluating segment rules
/// </summary>
public interface ISegmentRuleEngine
{
    /// <summary>
    /// Evaluates segment rules and returns matching entity IDs
    /// </summary>
    Task<SegmentEvaluationResult> EvaluateSegmentAsync(
        SegmentDefinition segmentDefinition,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates segment definition JSON schema
    /// </summary>
    Task<SegmentValidationResult> ValidateSegmentDefinitionAsync(
        string definitionJson,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses segment definition from JSON
    /// </summary>
    Task<SegmentDefinition?> ParseSegmentDefinitionAsync(
        string definitionJson,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of segment evaluation
/// </summary>
public class SegmentEvaluationResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<SegmentMatch> Matches { get; set; } = new();
    public TimeSpan EvaluationDuration { get; set; }
    public int TotalEntitiesEvaluated { get; set; }
}

/// <summary>
/// Represents a matched entity
/// </summary>
public class SegmentMatch
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public Dictionary<string, object> MatchedFields { get; set; } = new();
}

/// <summary>
/// Result of segment definition validation
/// </summary>
public class SegmentValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> AvailableFields { get; set; } = new();
}
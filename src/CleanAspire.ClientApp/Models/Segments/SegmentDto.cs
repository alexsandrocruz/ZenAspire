using System.Text.Json.Serialization;

namespace CleanAspire.ClientApp.Models.Segments;

/// <summary>
/// DTO for displaying segment information
/// </summary>
public class SegmentDto
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

    [JsonPropertyName("lastRebuiltAt")]
    public DateTime? LastRebuiltAt { get; set; }

    [JsonPropertyName("memberCount")]
    public int? MemberCount { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("lastModified")]
    public DateTime? LastModified { get; set; }

    [JsonPropertyName("lastModifiedBy")]
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Computed display property for formatted last rebuild time
    /// </summary>
    public string LastRebuiltDisplay => LastRebuiltAt?.ToString("g") ?? "Never";

    /// <summary>
    /// Computed display property for status
    /// </summary>
    public string StatusDisplay => IsActive ? "Active" : "Inactive";

    /// <summary>
    /// Computed display property for member count
    /// </summary>
    public string MemberCountDisplay => MemberCount?.ToString("N0") ?? "Unknown";
}
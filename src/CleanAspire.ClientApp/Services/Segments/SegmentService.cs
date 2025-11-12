using CleanAspire.ClientApp.Models.Segments;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.Json;

namespace CleanAspire.ClientApp.Services.Segments;

/// <summary>
/// Service for interacting with segment APIs
/// </summary>
public class SegmentService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public SegmentService(HttpClient http)
    {
        _http = http;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Get all segments
    /// </summary>
    public async Task<List<SegmentDto>> GetAllSegmentsAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/segments");
            response.EnsureSuccessStatusCode();

            var segments = await response.Content.ReadFromJsonAsync<List<SegmentDto>>(_jsonOptions);
            return segments ?? new List<SegmentDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching segments: {ex.Message}");
            return new List<SegmentDto>();
        }
    }

    /// <summary>
    /// Get segment by ID
    /// </summary>
    public async Task<SegmentDto?> GetSegmentByIdAsync(Guid id)
    {
        try
        {
            var response = await _http.GetAsync($"api/segments/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SegmentDto>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching segment {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get segment statistics
    /// </summary>
    public async Task<SegmentStatsDto?> GetSegmentStatsAsync(Guid id)
    {
        try
        {
            var response = await _http.GetAsync($"api/segments/{id}/stats");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SegmentStatsDto>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching segment stats {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create a new segment
    /// </summary>
    public async Task<SegmentDto?> CreateSegmentAsync(CreateSegmentCommand command)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/segments", command, _jsonOptions);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SegmentDto>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating segment: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update an existing segment
    /// </summary>
    public async Task<SegmentDto?> UpdateSegmentAsync(UpdateSegmentCommand command)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/segments/{command.Id}", command, _jsonOptions);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SegmentDto>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating segment {command.Id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Delete a segment
    /// </summary>
    public async Task<bool> DeleteSegmentAsync(Guid id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/segments/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting segment {id}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Rebuild a segment
    /// </summary>
    public async Task<SegmentStatsDto?> RebuildSegmentAsync(Guid id, bool forceRebuild = false)
    {
        try
        {
            var response = await _http.PostAsync($"api/segments/{id}/rebuild?forceRebuild={forceRebuild}", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SegmentStatsDto>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rebuilding segment {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get available fields for segment rules
    /// </summary>
    public async Task<SegmentFieldsResponseDto?> GetAvailableFieldsAsync(string? ownerType = null)
    {
        try
        {
            var url = "api/segments/fields";
            if (!string.IsNullOrEmpty(ownerType))
            {
                url += $"?ownerType={ownerType}";
            }

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SegmentFieldsResponseDto>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching available fields: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Validate segment definition
    /// </summary>
    public async Task<SegmentValidationResultDto?> ValidateSegmentDefinitionAsync(string definitionJson)
    {
        try
        {
            var command = new { DefinitionJson = definitionJson };
            var response = await _http.PostAsJsonAsync("api/segments/validate", command, _jsonOptions);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SegmentValidationResultDto>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating segment definition: {ex.Message}");
            return new SegmentValidationResultDto
            {
                IsValid = false,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Rebuild all active segments
    /// </summary>
    public async Task<List<object>?> RebuildAllSegmentsAsync()
    {
        try
        {
            var response = await _http.PostAsync("api/segments/rebuild-all", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<object>>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rebuilding all segments: {ex.Message}");
            return null;
        }
    }
}
using System.Net.Http.Json;
using CleanAspire.ClientApp.DTOs;

namespace CleanAspire.ClientApp.Services.Activities;

/// <summary>
/// Service proxy for managing activity operations.
/// </summary>
public class ActivityServiceProxy
{
    private readonly HttpClient _httpClient;

    public ActivityServiceProxy(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get activities with pagination
    /// </summary>
    public async Task<PaginatedResult<ActivityDto>?> GetActivitiesWithPaginationAsync(
        int pageNumber = 0,
        int pageSize = 10,
        string? searchTerm = null,
        ActivityStatus? status = null,
        ActivityType? type = null,
        ActivityPriority? priority = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        RegardingType? regardingType = null,
        Guid? regardingId = null)
    {
        try
        {
            var query = new
            {
                Keywords = searchTerm ?? "",
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy = "Start",
                SortDirection = "Descending",
                FilterByStatus = status,
                FilterByType = type,
                FilterByPriority = priority,
                FilterByRegardingType = regardingType,
                FilterByRegardingId = regardingId,
                FilterStartFrom = startDate,
                FilterStartTo = endDate
            };

            var response = await _httpClient.PostAsJsonAsync("/api/crm/activities/search", query);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error getting paginated activities:");
                Console.WriteLine($"   Status: {response.StatusCode} ({(int)response.StatusCode})");
                Console.WriteLine($"   URL: {response.RequestMessage?.RequestUri}");
                Console.WriteLine($"   Error: {errorContent}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<PaginatedResult<ActivityDto>>();
            Console.WriteLine($"✅ Got {result?.Items?.Count() ?? 0} activities successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting paginated activities: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get activity by ID
    /// </summary>
    public async Task<ActivityDto?> GetActivityByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/crm/activities/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting activity by ID: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ActivityDto>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting activity by ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create a new activity
    /// </summary>
    public async Task<ActivityDto?> CreateActivityAsync(ActivityRequest activityRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/crm/activities", activityRequest);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error creating activity: {response.StatusCode} - {errorContent}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ActivityDto>();
            Console.WriteLine($"✅ Activity created successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating activity: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update an existing activity
    /// </summary>
    public async Task<bool> UpdateActivityAsync(Guid id, ActivityRequest activityRequest)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/crm/activities/{id}", activityRequest);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error updating activity: {response.StatusCode} - {errorContent}");
                return false;
            }

            Console.WriteLine($"✅ Activity updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating activity: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete an activity
    /// </summary>
    public async Task<bool> DeleteActivityAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/crm/activities/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error deleting activity: {response.StatusCode}");
                return false;
            }

            Console.WriteLine($"✅ Activity deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting activity: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Complete an activity
    /// </summary>
    public async Task<bool> CompleteActivityAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/crm/activities/{id}/complete", null);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error completing activity: {response.StatusCode}");
                return false;
            }

            Console.WriteLine($"✅ Activity completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error completing activity: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get activities for a specific regarding entity
    /// </summary>
    public async Task<PaginatedResult<ActivityDto>?> GetActivitiesByRegardingAsync(
        RegardingType regardingType,
        Guid regardingId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await GetActivitiesWithPaginationAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            regardingType: regardingType,
            regardingId: regardingId);
    }
}

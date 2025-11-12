using CleanAspire.ClientApp.Services.Activities.DTOs;
using System.Net.Http.Json;

namespace CleanAspire.ClientApp.Services.Activities;

/// <summary>
/// Service proxy for activity reminders API
/// Part of Phase 3: Timeline - Basic Reminder System
/// </summary>
public class ReminderServiceProxy
{
    private readonly HttpClient _http;

    public ReminderServiceProxy(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Get pending reminders for current user
    /// </summary>
    public async Task<List<ActivityReminderDto>?> GetPendingRemindersAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ActivityReminderDto>>("/api/activities/reminders/pending");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting pending reminders: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get upcoming reminders within time window
    /// </summary>
    public async Task<List<ActivityReminderDto>?> GetUpcomingRemindersAsync(int hours = 24)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ActivityReminderDto>>($"/api/activities/reminders/upcoming?hours={hours}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting upcoming reminders: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Mark reminder as sent
    /// </summary>
    public async Task<bool> MarkReminderAsSentAsync(string activityId)
    {
        try
        {
            var response = await _http.PostAsync($"/api/activities/reminders/{activityId}/mark-sent", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking reminder as sent: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Process pending reminders (admin endpoint)
    /// </summary>
    public async Task<int?> ProcessPendingRemindersAsync()
    {
        try
        {
            var response = await _http.PostAsync("/api/activities/reminders/process", null);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ProcessRemindersResult>();
                return result?.ProcessedCount;
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing pending reminders: {ex.Message}");
            return null;
        }
    }

    private class ProcessRemindersResult
    {
        public int ProcessedCount { get; set; }
    }
}
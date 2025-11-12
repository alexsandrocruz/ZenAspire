using CleanAspire.Application.Features.Activities.DTOs;

namespace CleanAspire.Application.Features.Activities.Services;

/// <summary>
/// Interface for reminder service that manages activity reminders
/// Part of Phase 3: Timeline - Basic Reminder System
/// </summary>
public interface IReminderService
{
    /// <summary>
    /// Get all pending reminders for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending reminders</returns>
    Task<List<ActivityReminderDto>> GetPendingRemindersAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upcoming reminders within a time window
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="from">Start time window</param>
    /// <param name="to">End time window</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of upcoming reminders</returns>
    Task<List<ActivityReminderDto>> GetUpcomingRemindersAsync(string userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process and send pending reminders
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of reminders processed</returns>
    Task<int> ProcessPendingRemindersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a reminder as sent
    /// </summary>
    /// <param name="activityId">Activity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success flag</returns>
    Task<bool> MarkReminderAsSentAsync(string activityId, CancellationToken cancellationToken = default);
}
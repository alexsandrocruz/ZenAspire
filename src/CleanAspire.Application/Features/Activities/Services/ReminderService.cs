using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Activities.DTOs;
using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanAspire.Application.Features.Activities.Services;

/// <summary>
/// Service for managing activity reminders
/// Part of Phase 3: Timeline - Basic Reminder System
/// </summary>
public class ReminderService : IReminderService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ReminderService> _logger;
    private readonly IDateTime _dateTime;

    public ReminderService(
        IApplicationDbContext context,
        ILogger<ReminderService> logger,
        IDateTime dateTime)
    {
        _context = context;
        _logger = logger;
        _dateTime = dateTime;
    }

    public async Task<List<ActivityReminderDto>> GetPendingRemindersAsync(string userId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        var activities = await _context.Activities
            .AsNoTracking()
            .Where(a => a.ReminderAt.HasValue &&
                       a.ReminderAt.Value <= utcNow &&
                       !a.ReminderSent &&
                       a.Status == ActivityStatus.Open &&
                       (string.IsNullOrEmpty(userId) || a.AssignedToUserId == userId))
            .OrderBy(a => a.ReminderAt!.Value)
            .ToListAsync(cancellationToken);

        return activities.Select(MapToReminderDto).ToList();
    }

    public async Task<List<ActivityReminderDto>> GetUpcomingRemindersAsync(string userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var activities = await _context.Activities
            .AsNoTracking()
            .Where(a => a.ReminderAt.HasValue &&
                       a.ReminderAt.Value >= from &&
                       a.ReminderAt.Value <= to &&
                       !a.ReminderSent &&
                       a.Status == ActivityStatus.Open &&
                       (string.IsNullOrEmpty(userId) || a.AssignedToUserId == userId))
            .OrderBy(a => a.ReminderAt!.Value)
            .ToListAsync(cancellationToken);

        return activities.Select(MapToReminderDto).ToList();
    }

    public async Task<int> ProcessPendingRemindersAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        var pendingReminders = await _context.Activities
            .Where(a => a.ReminderAt.HasValue &&
                       a.ReminderAt.Value <= utcNow &&
                       !a.ReminderSent &&
                       a.Status == ActivityStatus.Open)
            .ToListAsync(cancellationToken);

        var processedCount = 0;

        foreach (var activity in pendingReminders)
        {
            try
            {
                // Mark reminder as sent
                activity.ReminderSent = true;

                // In a real implementation, this would send notifications:
                // - Email notification
                // - Push notification
                // - In-app notification
                // - SMS/WhatsApp notification based on user preferences

                _logger.LogInformation("Reminder sent for activity {ActivityId}: {Subject}", activity.Id, activity.Subject);

                processedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminder for activity {ActivityId}", activity.Id);
            }
        }

        if (processedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Processed {Count} reminders", processedCount);
        }

        return processedCount;
    }

    public async Task<bool> MarkReminderAsSentAsync(string activityId, CancellationToken cancellationToken = default)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.Id == activityId, cancellationToken);

        if (activity == null)
        {
            _logger.LogWarning("Activity {ActivityId} not found when marking reminder as sent", activityId);
            return false;
        }

        activity.ReminderSent = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reminder marked as sent for activity {ActivityId}", activityId);
        return true;
    }

    private static ActivityReminderDto MapToReminderDto(Activity activity)
    {
        var now = DateTime.UtcNow;
        var timeUntil = activity.ReminderAt.HasValue ? activity.ReminderAt.Value - now : TimeSpan.Zero;

        return new ActivityReminderDto
        {
            Id = activity.Id,
            Subject = activity.Subject,
            Description = activity.Description,
            Type = activity.Type,
            Priority = activity.Priority,
            ReminderAt = activity.ReminderAt ?? DateTime.MinValue,
            Start = activity.Start,
            Due = activity.Due,
            RegardingType = activity.RegardingType,
            RegardingId = activity.RegardingId,
            RegardingName = GetRegardingName(activity),
            AssignedToUserId = activity.AssignedToUserId,
            Location = activity.Location,
            ReminderSent = activity.ReminderSent,
            TimeUntilReminder = FormatTimeUntil(timeUntil)
        };
    }

    private static string? GetRegardingName(Activity activity)
    {
        // In a real implementation, this would query the related entity
        // For now, return a formatted string based on type
        return activity.RegardingType switch
        {
            RegardingType.Client => $"Client {activity.RegardingId}",
            RegardingType.Contact => $"Contact {activity.RegardingId}",
            RegardingType.Opportunity => $"Opportunity {activity.RegardingId}",
            RegardingType.Case => $"Case {activity.RegardingId}",
            RegardingType.Lead => $"Lead {activity.RegardingId}",
            RegardingType.Campaign => $"Campaign {activity.RegardingId}",
            RegardingType.Project => $"Project {activity.RegardingId}",
            _ => null
        };
    }

    private static string FormatTimeUntil(TimeSpan timeUntil)
    {
        if (timeUntil <= TimeSpan.Zero)
            return "Agora";

        if (timeUntil.TotalMinutes < 60)
            return $"em {timeUntil.Minutes} minuto{(timeUntil.Minutes != 1 ? "s" : "")}";

        if (timeUntil.TotalHours < 24)
            return $"em {timeUntil.Hours} hora{(timeUntil.Hours != 1 ? "s" : "")}";

        if (timeUntil.TotalDays < 7)
            return $"em {timeUntil.Days} dia{(timeUntil.Days != 1 ? "s" : "")}";

        return $"em {timeUntil.Days / 7} semana{(timeUntil.Days / 7 != 1 ? "s" : "")}";
    }
}
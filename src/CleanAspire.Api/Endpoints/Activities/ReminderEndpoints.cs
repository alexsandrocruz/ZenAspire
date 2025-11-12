using CleanAspire.Application.Features.Activities.DTOs;
using CleanAspire.Application.Features.Activities.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CleanAspire.Api.Endpoints.Activities;

/// <summary>
/// API endpoints for activity reminders
/// Part of Phase 3: Timeline - Basic Reminder System
/// </summary>
public class ReminderEndpoints : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/activities/reminders")
            .WithTags("Reminders")
            .RequireAuthorization();

        // Get pending reminders for current user
        group.MapGet("/pending", GetPendingRemindersAsync)
            .WithName("GetPendingReminders")
            .WithSummary("Get pending reminders")
            .WithDescription("Get all pending reminders for the current user");

        // Get upcoming reminders
        group.MapGet("/upcoming", GetUpcomingRemindersAsync)
            .WithName("GetUpcomingReminders")
            .WithSummary("Get upcoming reminders")
            .WithDescription("Get upcoming reminders within a time window");

        // Mark reminder as sent
        group.MapPost("/{activityId:guid}/mark-sent", MarkReminderAsSentAsync)
            .WithName("MarkReminderAsSent")
            .WithSummary("Mark reminder as sent")
            .WithDescription("Mark a reminder as sent for an activity");

        // Process pending reminders (admin/management endpoint)
        group.MapPost("/process", ProcessPendingRemindersAsync)
            .WithName("ProcessPendingReminders")
            .WithSummary("Process pending reminders")
            .WithDescription("Process all pending reminders (admin endpoint)")
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> GetPendingRemindersAsync(
        [FromServices] IReminderService reminderService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var reminders = await reminderService.GetPendingRemindersAsync(userId, cancellationToken);
        return Results.Ok(reminders);
    }

    private static async Task<IResult> GetUpcomingRemindersAsync(
        [FromServices] IReminderService reminderService,
        HttpContext context,
        CancellationToken cancellationToken,
        [FromQuery] int hours = 24)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var from = DateTime.UtcNow;
        var to = from.AddHours(hours);

        var reminders = await reminderService.GetUpcomingRemindersAsync(userId, from, to, cancellationToken);
        return Results.Ok(reminders);
    }

    private static async Task<IResult> MarkReminderAsSentAsync(
        string activityId,
        [FromServices] IReminderService reminderService,
        CancellationToken cancellationToken)
    {
        var success = await reminderService.MarkReminderAsSentAsync(activityId, cancellationToken);

        if (!success)
            return Results.NotFound($"Activity with ID {activityId} not found.");

        return Results.NoContent();
    }

    private static async Task<IResult> ProcessPendingRemindersAsync(
        [FromServices] IReminderService reminderService,
        CancellationToken cancellationToken)
    {
        var processedCount = await reminderService.ProcessPendingRemindersAsync(cancellationToken);
        return Results.Ok(new { ProcessedCount = processedCount });
    }
}
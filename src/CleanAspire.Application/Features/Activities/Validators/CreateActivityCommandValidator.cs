// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Activities.Commands;
using FluentValidation;

namespace CleanAspire.Application.Features.Activities.Validators;

/// <summary>
/// Validator for CreateActivityCommand.
/// Uses FluentValidation to apply validation rules for activity creation.
/// </summary>
public class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    /// <summary>
    /// Initializes validation rules for activity creation fields.
    /// </summary>
    public CreateActivityCommandValidator()
    {
        // Validate Subject (required, max length 160)
        RuleFor(command => command.Subject)
            .NotEmpty().WithMessage("Activity subject is required.")
            .MaximumLength(160).WithMessage("Subject must not exceed 160 characters.");

        // Validate Description (max length 2000)
        RuleFor(command => command.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(command => !string.IsNullOrEmpty(command.Description));

        // Validate Start date (required)
        RuleFor(command => command.Start)
            .NotEmpty().WithMessage("Start date is required.");

        // Validate Due date (must be after Start if provided)
        RuleFor(command => command.Due)
            .GreaterThan(command => command.Start)
            .WithMessage("Due date must be after start date.")
            .When(command => command.Due.HasValue);

        // Validate ReminderAt (must be before Due if both provided)
        RuleFor(command => command.ReminderAt)
            .LessThan(command => command.Due!.Value)
            .WithMessage("Reminder must be before due date.")
            .When(command => command.ReminderAt.HasValue && command.Due.HasValue);

        // Validate Location (max length 200)
        RuleFor(command => command.Location)
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters.")
            .When(command => !string.IsNullOrEmpty(command.Location));

        // Validate DurationMinutes (must be positive if provided)
        RuleFor(command => command.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes.")
            .When(command => command.DurationMinutes.HasValue);

        // Validate Type (must be defined enum value)
        RuleFor(command => command.Type)
            .IsInEnum().WithMessage("Invalid activity type.");

        // Validate Status (must be defined enum value)
        RuleFor(command => command.Status)
            .IsInEnum().WithMessage("Invalid activity status.");

        // Validate Priority (must be defined enum value)
        RuleFor(command => command.Priority)
            .IsInEnum().WithMessage("Invalid activity priority.");

        // Validate RegardingType (must be defined enum value if provided)
        RuleFor(command => command.RegardingType)
            .IsInEnum().WithMessage("Invalid regarding type.")
            .When(command => command.RegardingType.HasValue);

        // Validate RegardingId (must be provided if RegardingType is set)
        RuleFor(command => command.RegardingId)
            .NotEmpty().WithMessage("Regarding ID is required when regarding type is specified.")
            .When(command => command.RegardingType.HasValue);

        // Validate AssignedToUserId (max length 450)
        RuleFor(command => command.AssignedToUserId)
            .MaximumLength(450).WithMessage("Assigned user ID must not exceed 450 characters.")
            .When(command => !string.IsNullOrEmpty(command.AssignedToUserId));
    }
}

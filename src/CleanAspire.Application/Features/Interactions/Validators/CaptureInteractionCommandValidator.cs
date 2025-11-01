// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Interactions.Commands;
using CleanAspire.Domain.Entities;
using FluentValidation;
using System.Text.Json;

namespace CleanAspire.Application.Features.Interactions.Validators;

/// <summary>
/// Validator for CaptureInteractionCommand.
/// Uses FluentValidation to apply validation rules for capturing interactions.
/// </summary>
public class CaptureInteractionCommandValidator : AbstractValidator<CaptureInteractionCommand>
{
    /// <summary>
    /// Initializes validation rules for capturing interactions.
    /// </summary>
    public CaptureInteractionCommandValidator()
    {
        // Validate Type (required, must be defined enum value)
        RuleFor(command => command.Type)
            .IsInEnum().WithMessage("Invalid interaction type.");

        // Validate Direction (required, must be valid)
        RuleFor(command => command.Direction)
            .NotEmpty().WithMessage("Direction is required.")
            .Must(BeValidDirection).WithMessage("Direction must be 'Inbound', 'Outbound', or 'Internal'.");

        // Validate At (required)
        RuleFor(command => command.At)
            .NotEmpty().WithMessage("Interaction date/time is required when provided.")
            .When(command => command.At.HasValue);

        // Validate OwnerType (required, must be defined enum value)
        RuleFor(command => command.OwnerType)
            .IsInEnum().WithMessage("Invalid owner type.");

        // Validate OwnerId (required, must not be empty)
        RuleFor(command => command.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");

        // Validate ChannelRef (max length 256)
        RuleFor(command => command.ChannelRef)
            .MaximumLength(256).WithMessage("Channel reference must not exceed 256 characters.")
            .When(command => !string.IsNullOrEmpty(command.ChannelRef));

        // Validate Subject (max length 200)
        RuleFor(command => command.Subject)
            .MaximumLength(200).WithMessage("Subject must not exceed 200 characters.")
            .When(command => !string.IsNullOrEmpty(command.Subject));

        // Validate Snippet (max length 500)
        RuleFor(command => command.Snippet)
            .MaximumLength(500).WithMessage("Snippet must not exceed 500 characters.")
            .When(command => !string.IsNullOrEmpty(command.Snippet));

        // Validate PayloadJson (must be valid JSON if provided)
        RuleFor(command => command.PayloadJson)
            .Must(BeValidJson).WithMessage("PayloadJson must be valid JSON.")
            .When(command => !string.IsNullOrEmpty(command.PayloadJson));

        // Validate DurationSeconds (must be positive if provided)
        RuleFor(command => command.DurationSeconds)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 seconds.")
            .When(command => command.DurationSeconds.HasValue);

        // Validate HandledByUserId (max length 450)
        RuleFor(command => command.HandledByUserId)
            .MaximumLength(450).WithMessage("Handled by user ID must not exceed 450 characters.")
            .When(command => !string.IsNullOrEmpty(command.HandledByUserId));

        // Validate Sentiment (must be valid if provided)
        RuleFor(command => command.Sentiment)
            .Must(BeValidSentiment).WithMessage("Sentiment must be 'Positive', 'Neutral', 'Negative', or 'Mixed'.")
            .When(command => !string.IsNullOrEmpty(command.Sentiment));

        // Validate Tags (max length 500)
        RuleFor(command => command.Tags)
            .MaximumLength(500).WithMessage("Tags must not exceed 500 characters.")
            .When(command => !string.IsNullOrEmpty(command.Tags));
    }

    /// <summary>
    /// Validates that the direction is one of the valid values.
    /// </summary>
    private bool BeValidDirection(string direction)
    {
        return direction == InteractionDirection.Inbound ||
               direction == InteractionDirection.Outbound ||
               direction == InteractionDirection.Internal;
    }

    /// <summary>
    /// Validates that the sentiment is one of the valid values.
    /// </summary>
    private bool BeValidSentiment(string? sentiment)
    {
        if (string.IsNullOrEmpty(sentiment))
            return true;

        return sentiment == InteractionSentiment.Positive ||
               sentiment == InteractionSentiment.Neutral ||
               sentiment == InteractionSentiment.Negative ||
               sentiment == InteractionSentiment.Mixed;
    }

    /// <summary>
    /// Validates that a string is valid JSON.
    /// </summary>
    private bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return true;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

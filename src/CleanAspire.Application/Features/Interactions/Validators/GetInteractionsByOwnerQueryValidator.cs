// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Interactions.Queries;
using CleanAspire.Domain.Entities;
using FluentValidation;

namespace CleanAspire.Application.Features.Interactions.Validators;

/// <summary>
/// Validator for GetInteractionsByOwnerQuery.
/// Uses FluentValidation to apply validation rules for querying interactions.
/// </summary>
public class GetInteractionsByOwnerQueryValidator : AbstractValidator<GetInteractionsByOwnerQuery>
{
    /// <summary>
    /// Initializes validation rules for querying interactions by owner.
    /// </summary>
    public GetInteractionsByOwnerQueryValidator()
    {
        // Validate OwnerType (required, must be defined enum value)
        RuleFor(query => query.OwnerType)
            .IsInEnum().WithMessage("Invalid owner type.");

        // Validate OwnerId (required, must not be empty)
        RuleFor(query => query.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");

        // Validate Type (must be defined enum value if provided)
        RuleFor(query => query.Type)
            .IsInEnum().WithMessage("Invalid interaction type.")
            .When(query => query.Type.HasValue);

        // Validate Direction (must be valid if provided)
        RuleFor(query => query.Direction)
            .Must(BeValidDirection).WithMessage("Direction must be 'Inbound', 'Outbound', or 'Internal'.")
            .When(query => !string.IsNullOrEmpty(query.Direction));

        // Validate date range (EndDate must be after StartDate)
        RuleFor(query => query.EndDate)
            .GreaterThanOrEqualTo(query => query.StartDate!.Value)
            .WithMessage("End date must be after or equal to start date.")
            .When(query => query.StartDate.HasValue && query.EndDate.HasValue);

        // Validate PageNumber (must be positive)
        RuleFor(query => query.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        // Validate PageSize (must be between 1 and 100)
        RuleFor(query => query.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }

    /// <summary>
    /// Validates that the direction is one of the valid values.
    /// </summary>
    private bool BeValidDirection(string? direction)
    {
        if (string.IsNullOrEmpty(direction))
            return true;

        return direction == InteractionDirection.Inbound ||
               direction == InteractionDirection.Outbound ||
               direction == InteractionDirection.Internal;
    }
}

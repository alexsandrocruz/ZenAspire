// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Timeline.Queries;
using FluentValidation;

namespace CleanAspire.Application.Features.Timeline.Validators;

/// <summary>
/// Validator for GetTimelineQuery.
/// Uses FluentValidation to apply validation rules for timeline queries.
/// </summary>
public class GetTimelineQueryValidator : AbstractValidator<GetTimelineQuery>
{
    /// <summary>
    /// Initializes validation rules for timeline query parameters.
    /// </summary>
    public GetTimelineQueryValidator()
    {
        // Validate OwnerType (required, must be defined enum value)
        RuleFor(query => query.OwnerType)
            .IsInEnum().WithMessage("Invalid owner type.");

        // Validate OwnerId (required, must not be empty)
        RuleFor(query => query.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.");

        // Validate TypeFilter (must be defined enum value if provided)
        RuleFor(query => query.TypeFilter)
            .IsInEnum().WithMessage("Invalid timeline item type filter.")
            .When(query => query.TypeFilter.HasValue);

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
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Activities.Commands;
using FluentValidation;

namespace CleanAspire.Application.Features.Activities.Validators;

/// <summary>
/// Validator for CompleteActivityCommand.
/// Uses FluentValidation to apply validation rules for activity completion.
/// </summary>
public class CompleteActivityCommandValidator : AbstractValidator<CompleteActivityCommand>
{
    /// <summary>
    /// Initializes validation rules for activity completion.
    /// </summary>
    public CompleteActivityCommandValidator()
    {
        // Validate Id (required)
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Activity ID is required.");

        // Validate CompletionNotes (max length 1000 if provided)
        RuleFor(command => command.CompletionNotes)
            .MaximumLength(1000).WithMessage("Completion notes must not exceed 1000 characters.")
            .When(command => !string.IsNullOrEmpty(command.CompletionNotes));
    }
}

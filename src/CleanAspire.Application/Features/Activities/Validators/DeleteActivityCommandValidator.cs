// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Activities.Commands;
using FluentValidation;

namespace CleanAspire.Application.Features.Activities.Validators;

/// <summary>
/// Validator for DeleteActivityCommand.
/// Uses FluentValidation to apply validation rules for activity deletion.
/// </summary>
public class DeleteActivityCommandValidator : AbstractValidator<DeleteActivityCommand>
{
    /// <summary>
    /// Initializes validation rules for activity deletion.
    /// </summary>
    public DeleteActivityCommandValidator()
    {
        // Validate Id (required)
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Activity ID is required.");
    }
}

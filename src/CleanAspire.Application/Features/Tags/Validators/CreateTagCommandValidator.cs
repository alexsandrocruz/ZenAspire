// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Tags.Commands;
using FluentValidation;

namespace CleanAspire.Application.Features.Tags.Validators;

/// <summary>
/// Validator for CreateTagCommand.
/// </summary>
public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required.")
            .MaximumLength(100).WithMessage("Tag name must not exceed 100 characters.");

        RuleFor(x => x.Color)
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex format (e.g., #FF5733).")
            .When(x => !string.IsNullOrEmpty(x.Color));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

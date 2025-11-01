// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Application.Features.Notes.Commands;
using FluentValidation;

namespace CleanAspire.Application.Features.Notes.Validators;

/// <summary>
/// Validator for UpdateNoteCommand.
/// </summary>
public class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Note ID is required.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Note body is required.");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Title));
    }
}

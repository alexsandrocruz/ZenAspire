// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using FluentValidation;

namespace CleanAspire.Application.Features.Channels.Commands;

/// <summary>
/// Validator for UpdateChannelIdentityCommand
/// </summary>
public class UpdateChannelIdentityCommandValidator : AbstractValidator<UpdateChannelIdentityCommand>
{
    public UpdateChannelIdentityCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Channel ID is required");

        RuleFor(v => v.Value)
            .NotEmpty().WithMessage("Channel value is required")
            .MaximumLength(256);

        RuleFor(v => v.Label)
            .MaximumLength(50);
    }
}

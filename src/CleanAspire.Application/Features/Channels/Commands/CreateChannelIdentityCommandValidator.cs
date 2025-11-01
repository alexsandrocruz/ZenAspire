// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CleanAspire.Domain.Enums;
using FluentValidation;

namespace CleanAspire.Application.Features.Channels.Commands;

/// <summary>
/// Validator for CreateChannelIdentityCommand
/// </summary>
public class CreateChannelIdentityCommandValidator : AbstractValidator<CreateChannelIdentityCommand>
{
    public CreateChannelIdentityCommandValidator()
    {
        RuleFor(v => v.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required")
            .MaximumLength(450);

        RuleFor(v => v.Value)
            .NotEmpty().WithMessage("Channel value is required")
            .MaximumLength(256);

        // Email-specific validation
        RuleFor(v => v.Value)
            .EmailAddress().WithMessage("Invalid email address")
            .When(v => v.Type == ChannelType.Email);

        // Phone/Mobile/WhatsApp validation (basic check for digits and + symbol)
        RuleFor(v => v.Value)
            .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Phone number contains invalid characters")
            .When(v => v.Type == ChannelType.Phone || v.Type == ChannelType.Mobile || v.Type == ChannelType.WhatsApp);

        // Website/social media URL validation
        RuleFor(v => v.Value)
            .Must(BeValidUrl).WithMessage("Invalid URL format")
            .When(v => v.Type == ChannelType.Website || v.Type == ChannelType.LinkedIn ||
                       v.Type == ChannelType.Facebook);

        RuleFor(v => v.Label)
            .MaximumLength(50);
    }

    private bool BeValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

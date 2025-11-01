// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using FluentValidation;

namespace CleanAspire.Application.Features.Addresses.Commands;

/// <summary>
/// Validator for UpdateAddressCommand
/// </summary>
public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Address ID is required");

        RuleFor(v => v.Line1)
            .NotEmpty().WithMessage("Address Line 1 is required")
            .MaximumLength(200);

        RuleFor(v => v.Line2)
            .MaximumLength(200);

        RuleFor(v => v.District)
            .MaximumLength(100);

        RuleFor(v => v.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100);

        RuleFor(v => v.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(50);

        RuleFor(v => v.Zip)
            .NotEmpty().WithMessage("ZIP/Postal code is required")
            .MaximumLength(20);

        RuleFor(v => v.Country)
            .NotEmpty().WithMessage("Country is required")
            .Length(2).WithMessage("Country code must be 2 characters (ISO 3166-1 alpha-2)");

        RuleFor(v => v.GeoLat)
            .InclusiveBetween(-90, 90).When(v => v.GeoLat.HasValue)
            .WithMessage("Latitude must be between -90 and +90");

        RuleFor(v => v.GeoLng)
            .InclusiveBetween(-180, 180).When(v => v.GeoLng.HasValue)
            .WithMessage("Longitude must be between -180 and +180");

        RuleFor(v => v.Label)
            .MaximumLength(50);
    }
}

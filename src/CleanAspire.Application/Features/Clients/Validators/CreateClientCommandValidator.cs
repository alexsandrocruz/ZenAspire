using CleanAspire.Application.Features.Clients.Commands;
using FluentValidation;

namespace CleanAspire.Application.Features.Clients.Validators;

/// <summary>
/// Validator for CreateClientCommand.
/// Uses FluentValidation to apply validation rules for client creation.
/// </summary>
public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    /// <summary>
    /// Initializes validation rules for client creation fields.
    /// </summary>
    public CreateClientCommandValidator()
    {
        // Validate Name (required, max length 200)
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Client name is required.")
            .MaximumLength(200).WithMessage("Client name must not exceed 200 characters.");

        // Validate Trade Name (max length 50)
        RuleFor(command => command.TradeName)
            .MaximumLength(50).WithMessage("Trade name must not exceed 50 characters.");

        // Validate Document Number (max length 20)
        RuleFor(command => command.DocumentNumber)
            .MaximumLength(20).WithMessage("Document number must not exceed 20 characters.");

        // Validate Email (valid email format, max length 100)
        RuleFor(command => command.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(command => !string.IsNullOrEmpty(command.Email))
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

        // Validate Phone (max length 20)
        RuleFor(command => command.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.");

        // Validate Website (max length 250)
        RuleFor(command => command.Website)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("Invalid website URL.")
            .When(command => !string.IsNullOrEmpty(command.Website))
            .MaximumLength(250).WithMessage("Website must not exceed 250 characters.");

        // Validate Address (max length 200)
        RuleFor(command => command.Address)
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");

        // Validate City (max length 100)
        RuleFor(command => command.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        // Validate State (max length 50)
        RuleFor(command => command.State)
            .MaximumLength(50).WithMessage("State must not exceed 50 characters.");

        // Validate Postal Code (max length 20)
        RuleFor(command => command.PostalCode)
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters.");

        // Validate Country (max length 100)
        RuleFor(command => command.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        // Validate Industry (max length 100)
        RuleFor(command => command.Industry)
            .MaximumLength(100).WithMessage("Industry must not exceed 100 characters.");

        // Validate Size (max length 50)
        RuleFor(command => command.Size)
            .MaximumLength(50).WithMessage("Company size must not exceed 50 characters.");

        // Validate Annual Revenue (must be positive if provided)
        RuleFor(command => command.AnnualRevenue)
            .GreaterThan(0).WithMessage("Annual revenue must be greater than 0.")
            .When(command => command.AnnualRevenue.HasValue);

        // Validate Employee Count (must be positive if provided)
        RuleFor(command => command.EmployeeCount)
            .GreaterThan(0).WithMessage("Employee count must be greater than 0.")
            .When(command => command.EmployeeCount.HasValue);

        // Validate Notes (max length 1000)
        RuleFor(command => command.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");

        // Validate Tags (max length 500)
        RuleFor(command => command.ClientTags)
            .MaximumLength(500).WithMessage("Tags must not exceed 500 characters.");
    }
}
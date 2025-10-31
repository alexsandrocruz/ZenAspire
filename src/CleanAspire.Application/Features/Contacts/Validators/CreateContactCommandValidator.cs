using CleanAspire.Application.Features.Contacts.Commands;
using FluentValidation;

namespace CleanAspire.Application.Features.Contacts.Validators;

/// <summary>
/// Validator for CreateContactCommand.
/// Uses FluentValidation to apply validation rules for contact creation.
/// </summary>
public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    /// <summary>
    /// Initializes validation rules for contact creation fields.
    /// </summary>
    public CreateContactCommandValidator()
    {
        // Validate FirstName (required, max length 100)
        RuleFor(command => command.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        // Validate LastName (required, max length 100)
        RuleFor(command => command.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        // Validate Email (required, valid email format, max length 150)
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(150).WithMessage("Email must not exceed 150 characters.");

        // Validate Phone (max length 20)
        RuleFor(command => command.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.");

        // Validate MobilePhone (max length 20)
        RuleFor(command => command.MobilePhone)
            .MaximumLength(20).WithMessage("Mobile phone must not exceed 20 characters.");

        // Validate JobTitle (max length 100)
        RuleFor(command => command.JobTitle)
            .MaximumLength(100).WithMessage("Job title must not exceed 100 characters.");

        // Validate Department (max length 100)
        RuleFor(command => command.Department)
            .MaximumLength(100).WithMessage("Department must not exceed 100 characters.");

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

        // Validate Notes (max length 1000)
        RuleFor(command => command.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");

        // Validate Tags (max length 500)
        RuleFor(command => command.ContactTags)
            .MaximumLength(500).WithMessage("Tags must not exceed 500 characters.");

        // Validate ClientId (required)
        RuleFor(command => command.ClientId)
            .NotEmpty().WithMessage("Client ID is required.");
    }
}

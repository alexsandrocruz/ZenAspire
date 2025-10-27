using CleanAspire.Application.Features.Customers.Commands;

namespace CleanAspire.Application.Features.Customers.Validators;

/// <summary>
/// Validator for CreateCustomerCommand.
/// Uses FluentValidation to apply validation rules for customer creation.
/// </summary>
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    /// <summary>
    /// Initializes validation rules for customer creation fields.
    /// </summary>
    public CreateCustomerCommandValidator()
    {
        // Validate FirstName (required, max length 100)
        RuleFor(command => command.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        // Validate LastName (required, max length 100)
        RuleFor(command => command.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        // Validate Email (required, valid email format, max length 200)
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");

        // Validate PhoneNumber (optional, max length 20)
        RuleFor(command => command.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(command => !string.IsNullOrEmpty(command.PhoneNumber));

        // Validate Address (optional, max length 500)
        RuleFor(command => command.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.")
            .When(command => !string.IsNullOrEmpty(command.Address));
    }
}
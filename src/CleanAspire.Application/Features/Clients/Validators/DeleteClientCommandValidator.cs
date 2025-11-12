using CleanAspire.Application.Features.Clients.Commands;
using FluentValidation;

namespace CleanAspire.Application.Features.Clients.Validators;

/// <summary>
/// Validator for DeleteClientCommand.
/// Uses FluentValidation to apply validation rules for client deletion.
/// </summary>
public class DeleteClientCommandValidator : AbstractValidator<DeleteClientCommand>
{
    /// <summary>
    /// Initializes validation rules for client deletion.
    /// </summary>
    public DeleteClientCommandValidator()
    {
        // Validate Id (required)
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Client ID is required.");
    }
}
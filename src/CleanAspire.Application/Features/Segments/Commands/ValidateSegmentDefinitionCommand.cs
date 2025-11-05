using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Application.Features.Segments.Services;
using CleanAspire.Application.Pipeline;

namespace CleanAspire.Application.Features.Segments.Commands;

/// <summary>
/// Command for validating a segment definition without creating a segment
/// </summary>
public record ValidateSegmentDefinitionCommand : IRequest<SegmentValidationResultDto>, IRequiresValidation
{
    /// <summary>
    /// JSON definition of segment rules to validate
    /// </summary>
    public required string DefinitionJson { get; init; }
}

/// <summary>
/// Handler for processing ValidateSegmentDefinitionCommand
/// </summary>
public class ValidateSegmentDefinitionCommandHandler : IRequestHandler<ValidateSegmentDefinitionCommand, SegmentValidationResultDto>
{
    private readonly ISegmentRuleEngine _ruleEngine;
    private readonly ICurrentUserService _currentUser;

    public ValidateSegmentDefinitionCommandHandler(
        ISegmentRuleEngine ruleEngine,
        ICurrentUserService currentUser)
    {
        _ruleEngine = ruleEngine;
        _currentUser = currentUser;
    }

    public async ValueTask<SegmentValidationResultDto> Handle(ValidateSegmentDefinitionCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        try
        {
            // Use the rule engine to validate the definition
            var validationResult = await _ruleEngine.ValidateSegmentDefinitionAsync(request.DefinitionJson, cancellationToken);

            var result = new SegmentValidationResultDto
            {
                IsValid = validationResult.IsValid,
                Errors = validationResult.Errors.ToList(),
                Warnings = validationResult.Warnings.ToList(),
                AvailableFields = validationResult.AvailableFields.ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            return new SegmentValidationResultDto
            {
                IsValid = false,
                Errors = new List<string> { ex.Message },
                Warnings = new List<string>(),
                AvailableFields = new List<string>()
            };
        }
    }
}

/// <summary>
/// Validator for ValidateSegmentDefinitionCommand
/// </summary>
public class ValidateSegmentDefinitionCommandValidator : AbstractValidator<ValidateSegmentDefinitionCommand>
{
    public ValidateSegmentDefinitionCommandValidator(ICurrentUserService currentUser)
    {
        RuleFor(x => x)
            .Must(_ => !string.IsNullOrEmpty(currentUser.TenantId))
            .WithMessage("TenantId is required");

        RuleFor(x => x.DefinitionJson)
            .NotEmpty()
            .WithMessage("DefinitionJson is required");

        RuleFor(x => x.DefinitionJson)
            .Must(json => IsValidJson(json))
            .WithMessage("DefinitionJson must be valid JSON");
    }

    private static bool IsValidJson(string jsonString)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
using CleanAspire.Application.Features.Segments.Commands;
using CleanAspire.Domain.Models;
using FluentValidation;

namespace CleanAspire.Application.Features.Segments.Validators;

/// <summary>
/// Validator for CreateSegmentCommand.
/// Uses FluentValidation to apply validation rules for segment creation.
/// </summary>
public class CreateSegmentCommandValidator : AbstractValidator<CreateSegmentCommand>
{
    /// <summary>
    /// Initializes validation rules for segment creation fields.
    /// </summary>
    public CreateSegmentCommandValidator()
    {
        // Validate Name (required, max length 200)
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Segment name is required.")
            .MaximumLength(200).WithMessage("Segment name must not exceed 200 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-_()]+$").WithMessage("Segment name contains invalid characters.");

        // Validate Description (max length 1000)
        RuleFor(command => command.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        // Validate DefinitionJson (required, valid JSON)
        RuleFor(command => command.DefinitionJson)
            .NotEmpty().WithMessage("Segment definition JSON is required.")
            .Must(BeValidJson).WithMessage("Segment definition must be valid JSON.")
            .Must(BeValidSegmentDefinition).WithMessage("Segment definition must follow the correct schema.");

        // Validate TargetOwnerTypes (at least one type if specified)
        RuleFor(command => command.TargetOwnerTypes)
            .Must(types => types == null || types.Any()).WithMessage("At least one target owner type must be specified.");

        // Validate IsActive
        RuleFor(command => command.IsActive)
            .NotNull().WithMessage("IsActive status must be specified.");
    }

    /// <summary>
    /// Validates that the input is valid JSON
    /// </summary>
    private static bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that the JSON follows the segment definition schema
    /// </summary>
    private static bool BeValidSegmentDefinition(string json)
    {
        try
        {
            var definition = Newtonsoft.Json.JsonConvert.DeserializeObject<SegmentDefinition>(json);
            return definition != null && definition.Rules != null;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Validator for UpdateSegmentCommand.
/// Uses FluentValidation to apply validation rules for segment updates.
/// </summary>
public class UpdateSegmentCommandValidator : AbstractValidator<UpdateSegmentCommand>
{
    /// <summary>
    /// Initializes validation rules for segment update fields.
    /// </summary>
    public UpdateSegmentCommandValidator()
    {
        // Validate Id (required, not empty)
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Segment ID is required.");

        // Validate Name (required, max length 200)
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Segment name is required.")
            .MaximumLength(200).WithMessage("Segment name must not exceed 200 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-_()]+$").WithMessage("Segment name contains invalid characters.");

        // Validate Description (max length 1000)
        RuleFor(command => command.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        // Validate DefinitionJson (required, valid JSON)
        RuleFor(command => command.DefinitionJson)
            .NotEmpty().WithMessage("Segment definition JSON is required.")
            .Must(BeValidJson).WithMessage("Segment definition must be valid JSON.")
            .Must(BeValidSegmentDefinition).WithMessage("Segment definition must follow the correct schema.");

        // Validate TargetOwnerTypes (at least one type if specified)
        RuleFor(command => command.TargetOwnerTypes)
            .Must(types => types == null || types.Any()).WithMessage("At least one target owner type must be specified.");

        // Validate IsActive
        RuleFor(command => command.IsActive)
            .NotNull().WithMessage("IsActive status must be specified.");
    }

    /// <summary>
    /// Validates that the input is valid JSON
    /// </summary>
    private static bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that the JSON follows the segment definition schema
    /// </summary>
    private static bool BeValidSegmentDefinition(string json)
    {
        try
        {
            var definition = Newtonsoft.Json.JsonConvert.DeserializeObject<SegmentDefinition>(json);
            return definition != null && definition.Rules != null;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Validator for DeleteSegmentCommand.
/// Uses FluentValidation to apply validation rules for segment deletion.
/// </summary>
public class DeleteSegmentCommandValidator : AbstractValidator<DeleteSegmentCommand>
{
    /// <summary>
    /// Initializes validation rules for segment deletion.
    /// </summary>
    public DeleteSegmentCommandValidator()
    {
        // Validate Id (required, not empty)
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Segment ID is required.");
    }
}

/// <summary>
/// Validator for RebuildSegmentCommand.
/// Uses FluentValidation to apply validation rules for segment rebuilding.
/// </summary>
public class RebuildSegmentCommandValidator : AbstractValidator<RebuildSegmentCommand>
{
    /// <summary>
    /// Initializes validation rules for segment rebuilding.
    /// </summary>
    public RebuildSegmentCommandValidator()
    {
        // Validate Id (required, not empty)
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Segment ID is required.");

        // Validate ForceRebuild
        RuleFor(command => command.ForceRebuild)
            .NotNull().WithMessage("ForceRebuild flag must be specified.");
    }
}
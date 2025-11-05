using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Segments.DTOs;
using CleanAspire.Application.Features.Segments.Services;
using CleanAspire.Application.Pipeline;
using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Segments.Queries;

/// <summary>
/// Query for getting available fields for segment rules
/// </summary>
public record GetSegmentFieldsQuery : IRequest<SegmentFieldsResponseDto>, IRequiresValidation
{
    public OwnerType? OwnerType { get; init; }
}

/// <summary>
/// Handler for processing GetSegmentFieldsQuery
/// </summary>
public class GetSegmentFieldsQueryHandler : IRequestHandler<GetSegmentFieldsQuery, SegmentFieldsResponseDto>
{
    private readonly ISegmentRuleEngine _ruleEngine;
    private readonly ICurrentUserService _currentUser;

    public GetSegmentFieldsQueryHandler(
        ISegmentRuleEngine ruleEngine,
        ICurrentUserService currentUser)
    {
        _ruleEngine = ruleEngine;
        _currentUser = currentUser;
    }

    public async ValueTask<SegmentFieldsResponseDto> Handle(GetSegmentFieldsQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var result = new SegmentFieldsResponseDto();

        // Get fields for specific entity type or all types
        var ownerTypes = request.OwnerType.HasValue
            ? new List<OwnerType> { request.OwnerType.Value }
            : new List<OwnerType> { OwnerType.Client, OwnerType.Contact };

        foreach (var ownerType in ownerTypes)
        {
            var entityTypeName = ownerType.ToString();
            var fields = SegmentFieldDefinitions.GetFieldsForEntityType(ownerType);

            var fieldDtos = fields.Select(field => new FieldDefinitionDto
            {
                Name = field.Key,
                DisplayName = field.Value.DisplayName,
                Type = field.Value.Type.Name,
                Description = field.Value.Description ?? string.Empty,
                IsRequired = false,
                AllowedValues = field.Value.Options ?? new List<string>()
            }).ToList();

            result.FieldsByEntityType[entityTypeName] = fieldDtos;
        }

        // Add supported operators
        result.SupportedOperators = GetSupportedOperators();

        return result;
    }

    private static List<OperatorDefinitionDto> GetSupportedOperators()
    {
        return new List<OperatorDefinitionDto>
        {
            new()
            {
                Name = "eq",
                DisplayName = "Equals",
                Description = "Value equals the specified value",
                SupportedTypes = new List<string> { "String", "Int32", "DateTime", "Boolean", "Double" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "ne",
                DisplayName = "Not Equals",
                Description = "Value does not equal the specified value",
                SupportedTypes = new List<string> { "String", "Int32", "DateTime", "Boolean", "Double" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "gt",
                DisplayName = "Greater Than",
                Description = "Value is greater than the specified value",
                SupportedTypes = new List<string> { "Int32", "DateTime", "Double" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "gte",
                DisplayName = "Greater Than or Equal",
                Description = "Value is greater than or equal to the specified value",
                SupportedTypes = new List<string> { "Int32", "DateTime", "Double" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "lt",
                DisplayName = "Less Than",
                Description = "Value is less than the specified value",
                SupportedTypes = new List<string> { "Int32", "DateTime", "Double" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "lte",
                DisplayName = "Less Than or Equal",
                Description = "Value is less than or equal to the specified value",
                SupportedTypes = new List<string> { "Int32", "DateTime", "Double" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "contains",
                DisplayName = "Contains",
                Description = "String contains the specified value",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "startswith",
                DisplayName = "Starts With",
                Description = "String starts with the specified value",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "endswith",
                DisplayName = "Ends With",
                Description = "String ends with the specified value",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "in",
                DisplayName = "In List",
                Description = "Value is in the specified list of values",
                SupportedTypes = new List<string> { "String", "Int32", "Boolean" },
                RequiresArrayValue = true
            }
        };
    }
}

/// <summary>
/// Validator for GetSegmentFieldsQuery
/// </summary>
public class GetSegmentFieldsQueryValidator : AbstractValidator<GetSegmentFieldsQuery>
{
    public GetSegmentFieldsQueryValidator(ICurrentUserService currentUser)
    {
        RuleFor(x => x)
            .Must(_ => !string.IsNullOrEmpty(currentUser.TenantId))
            .WithMessage("TenantId is required");

        When(x => x.OwnerType.HasValue, () =>
        {
            RuleFor(x => x.OwnerType)
                .IsInEnum()
                .WithMessage("OwnerType must be a valid enum value");
        });
    }
}
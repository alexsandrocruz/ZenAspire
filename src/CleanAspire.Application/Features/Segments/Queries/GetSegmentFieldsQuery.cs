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
            // Basic comparison operators
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

            // Numeric/date comparison operators
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

            // String operators
            new()
            {
                Name = "contains",
                DisplayName = "Contains",
                Description = "String contains the specified value (case-insensitive)",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "notcontains",
                DisplayName = "Does Not Contain",
                Description = "String does not contain the specified value (case-insensitive)",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "startswith",
                DisplayName = "Starts With",
                Description = "String starts with the specified value (case-insensitive)",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "endswith",
                DisplayName = "Ends With",
                Description = "String ends with the specified value (case-insensitive)",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },

            // Collection operators
            new()
            {
                Name = "in",
                DisplayName = "In List",
                Description = "Value is in the specified list of values (JSON array or comma-separated)",
                SupportedTypes = new List<string> { "String", "Int32", "Boolean" },
                RequiresArrayValue = true
            },
            new()
            {
                Name = "notin",
                DisplayName = "Not In List",
                Description = "Value is not in the specified list of values",
                SupportedTypes = new List<string> { "String", "Int32", "Boolean" },
                RequiresArrayValue = true
            },

            // Range operators
            new()
            {
                Name = "between",
                DisplayName = "Between",
                Description = "Value is between the specified range (format: min,max)",
                SupportedTypes = new List<string> { "String", "Int32", "DateTime", "Double" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "notbetween",
                DisplayName = "Not Between",
                Description = "Value is not between the specified range",
                SupportedTypes = new List<string> { "String", "Int32", "DateTime", "Double" },
                RequiresArrayValue = false
            },

            // Null/Empty operators
            new()
            {
                Name = "isnull",
                DisplayName = "Is Null",
                Description = "Value is null",
                SupportedTypes = new List<string> { "String", "Int32", "DateTime", "Boolean", "Double" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "isnotnull",
                DisplayName = "Is Not Null",
                Description = "Value is not null",
                SupportedTypes = new List<string> { "String", "Int32", "DateTime", "Boolean", "Double" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "isempty",
                DisplayName = "Is Empty",
                Description = "String value is empty or null",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "isnotempty",
                DisplayName = "Is Not Empty",
                Description = "String value is not empty and not null",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },

            // Regex operators
            new()
            {
                Name = "regex",
                DisplayName = "Regex Match",
                Description = "String matches the regular expression pattern",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "notregex",
                DisplayName = "Does Not Match Regex",
                Description = "String does not match the regular expression pattern",
                SupportedTypes = new List<string> { "String" },
                RequiresArrayValue = false
            },

            // Date operators
            new()
            {
                Name = "today",
                DisplayName = "Is Today",
                Description = "Date is today",
                SupportedTypes = new List<string> { "DateTime" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "yesterday",
                DisplayName = "Is Yesterday",
                Description = "Date is yesterday",
                SupportedTypes = new List<string> { "DateTime" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "thisweek",
                DisplayName = "Is This Week",
                Description = "Date is within the current week",
                SupportedTypes = new List<string> { "DateTime" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "thismonth",
                DisplayName = "Is This Month",
                Description = "Date is within the current month",
                SupportedTypes = new List<string> { "DateTime" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "thisyear",
                DisplayName = "Is This Year",
                Description = "Date is within the current year",
                SupportedTypes = new List<string> { "DateTime" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "lastndays",
                DisplayName = "Last N Days",
                Description = "Date is within the last N days (value = number of days)",
                SupportedTypes = new List<string> { "DateTime" },
                RequiresArrayValue = false
            },
            new()
            {
                Name = "lastnmonths",
                DisplayName = "Last N Months",
                Description = "Date is within the last N months (value = number of months)",
                SupportedTypes = new List<string> { "DateTime" },
                RequiresArrayValue = false
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
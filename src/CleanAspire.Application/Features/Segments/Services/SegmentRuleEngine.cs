using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using CleanAspire.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace CleanAspire.Application.Features.Segments.Services;

/// <summary>
/// Implementation of segment rule engine using standard LINQ
/// </summary>
public class SegmentRuleEngine : ISegmentRuleEngine
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SegmentRuleEngine> _logger;

    public SegmentRuleEngine(
        IApplicationDbContext context,
        ILogger<SegmentRuleEngine> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SegmentEvaluationResult> EvaluateSegmentAsync(
        SegmentDefinition segmentDefinition,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new SegmentEvaluationResult();

        try
        {
            _logger.LogInformation("Starting segment evaluation for: {SegmentName}", segmentDefinition.Name);

            // Get target owner types
            var targetOwnerTypes = segmentDefinition.TargetOwnerTypes?.Any() == true
                ? segmentDefinition.TargetOwnerTypes
                : new List<OwnerType> { OwnerType.Client, OwnerType.Contact };

            var allMatches = new List<SegmentMatch>();

            // Evaluate for each target owner type
            foreach (var ownerType in targetOwnerTypes)
            {
                var matches = await EvaluateSegmentForOwnerTypeAsync(segmentDefinition, ownerType, cancellationToken);
                allMatches.AddRange(matches);

                _logger.LogInformation("Found {MatchCount} matches for {OwnerType}", matches.Count, ownerType);
            }

            result.Success = true;
            result.Matches = allMatches;
            result.TotalEntitiesEvaluated = await GetTotalEntitiesCountAsync(targetOwnerTypes, cancellationToken);

            _logger.LogInformation("Segment evaluation completed. Found {TotalMatches} total matches in {Duration}ms",
                allMatches.Count, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating segment: {SegmentName}", segmentDefinition.Name);
            result.Success = false;
            result.Errors.Add(ex.Message);
        }
        finally
        {
            stopwatch.Stop();
            result.EvaluationDuration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<SegmentValidationResult> ValidateSegmentDefinitionAsync(
        string definitionJson,
        CancellationToken cancellationToken = default)
    {
        var result = new SegmentValidationResult();

        try
        {
            // Parse JSON
            var segmentDefinition = await ParseSegmentDefinitionAsync(definitionJson, cancellationToken);
            if (segmentDefinition == null)
            {
                result.Errors.Add("Invalid JSON format");
                return result;
            }

            // Validate basic structure
            if (string.IsNullOrWhiteSpace(segmentDefinition.Name))
            {
                result.Errors.Add("Segment name is required");
            }

            if (segmentDefinition.Rules == null)
            {
                result.Errors.Add("Rules are required");
            }

            // Validate rules
            if (segmentDefinition.Rules != null)
            {
                ValidateRules(segmentDefinition.Rules, result);
            }

            // Validate target owner types
            if (segmentDefinition.TargetOwnerTypes != null && segmentDefinition.TargetOwnerTypes.Any())
            {
                foreach (var ownerType in segmentDefinition.TargetOwnerTypes)
                {
                    var fields = SegmentFieldDefinitions.GetFieldsForEntityType(ownerType);
                    result.AvailableFields.AddRange(fields.Keys);
                }
            }

            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating segment definition");
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<SegmentDefinition?> ParseSegmentDefinitionAsync(
        string definitionJson,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await Task.Run(() =>
            {
                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                };

                return JsonConvert.DeserializeObject<SegmentDefinition>(definitionJson, settings);
            }, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing segment definition JSON");
            return null;
        }
    }

    /// <summary>
    /// Evaluates segment rules for specific owner type using simple filtering
    /// </summary>
    private async Task<List<SegmentMatch>> EvaluateSegmentForOwnerTypeAsync(
        SegmentDefinition segmentDefinition,
        OwnerType ownerType,
        CancellationToken cancellationToken)
    {
        var matches = new List<SegmentMatch>();

        try
        {
            if (ownerType == OwnerType.Client)
            {
                var clients = await _context.Clients.ToListAsync(cancellationToken);
                var filteredClients = FilterEntities(clients, segmentDefinition.Rules);

                matches.AddRange(filteredClients.Select(client => new SegmentMatch
                {
                    EntityType = "Client",
                    EntityId = client.Id.ToString(),
                    MatchedFields = new Dictionary<string, object>
                    {
                        { "Name", client.Name },
                        { "ClientType", client.Type.ToString() }
                    }
                }));
            }
            else if (ownerType == OwnerType.Contact)
            {
                var contacts = await _context.Contacts.ToListAsync(cancellationToken);
                var filteredContacts = FilterEntities(contacts, segmentDefinition.Rules);

                matches.AddRange(filteredContacts.Select(contact => new SegmentMatch
                {
                    EntityType = "Contact",
                    EntityId = contact.Id.ToString(),
                    MatchedFields = new Dictionary<string, object>
                    {
                        { "Name", $"{contact.FirstName} {contact.LastName}" },
                        { "Email", contact.Email }
                    }
                }));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating segment for owner type: {OwnerType}", ownerType);
        }

        return matches;
    }

    /// <summary>
    /// Simple filter method that evaluates rules against entities
    /// </summary>
    private List<T> FilterEntities<T>(List<T> entities, SegmentRuleGroup ruleGroup) where T : class
    {
        return entities.Where(entity => EvaluateRuleGroup(entity, ruleGroup)).ToList();
    }

    /// <summary>
    /// Evaluates a rule group against an entity
    /// </summary>
    private bool EvaluateRuleGroup<T>(T entity, SegmentRuleGroup ruleGroup) where T : class
    {
        // Handle All rules (AND)
        if (ruleGroup.All?.Any() == true)
        {
            foreach (var rule in ruleGroup.All)
            {
                if (!EvaluateRule(entity, rule))
                    return false;
            }
        }

        // Handle Any rules (OR)
        bool anyMatch = false;
        if (ruleGroup.Any?.Any() == true)
        {
            foreach (var rule in ruleGroup.Any)
            {
                if (EvaluateRule(entity, rule))
                {
                    anyMatch = true;
                    break;
                }
            }
            if (!anyMatch && ruleGroup.Any.Any())
                return false;
        }

        // Handle Not rules
        if (ruleGroup.Not?.Any() == true)
        {
            foreach (var rule in ruleGroup.Not)
            {
                if (EvaluateRule(entity, rule))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Evaluates individual rule against an entity
    /// </summary>
    private bool EvaluateRule<T>(T entity, SegmentRuleBase rule) where T : class
    {
        if (rule is SegmentCondition condition)
        {
            return EvaluateCondition(entity, condition);
        }
        else if (rule is SegmentRuleGroup group)
        {
            return EvaluateRuleGroup(entity, group);
        }

        return false;
    }

    /// <summary>
    /// Evaluates a condition against an entity with advanced operators
    /// </summary>
    private bool EvaluateCondition<T>(T entity, SegmentCondition condition) where T : class
    {
        try
        {
            var property = entity.GetType().GetProperty(condition.Field);
            if (property == null)
                return false;

            var entityValue = property.GetValue(entity);
            var operation = condition.Operation.ToLowerInvariant();

            return operation switch
            {
                // Basic comparison operators
                "eq" => Equals(entityValue, condition.Value),
                "ne" => !Equals(entityValue, condition.Value),

                // Numeric comparison operators
                "gt" => CompareValues(entityValue, condition.Value) > 0,
                "gte" => CompareValues(entityValue, condition.Value) >= 0,
                "lt" => CompareValues(entityValue, condition.Value) < 0,
                "lte" => CompareValues(entityValue, condition.Value) <= 0,

                // String operators
                "contains" => entityValue?.ToString()?.IndexOf(condition.Value?.ToString() ?? "", StringComparison.OrdinalIgnoreCase) >= 0,
                "startswith" => entityValue?.ToString()?.StartsWith(condition.Value?.ToString() ?? "", StringComparison.OrdinalIgnoreCase) ?? false,
                "endswith" => entityValue?.ToString()?.EndsWith(condition.Value?.ToString() ?? "", StringComparison.OrdinalIgnoreCase) ?? false,
                "notcontains" => !(entityValue?.ToString()?.IndexOf(condition.Value?.ToString() ?? "", StringComparison.OrdinalIgnoreCase) >= 0),

                // Collection operators
                "in" => IsInList(entityValue, condition.Value),
                "notin" => !IsInList(entityValue, condition.Value),

                // Range operators
                "between" => IsBetween(entityValue, condition.Value),
                "notbetween" => !IsBetween(entityValue, condition.Value),

                // Null/Empty operators
                "isnull" => entityValue == null,
                "isnotnull" => entityValue != null,
                "isempty" => string.IsNullOrEmpty(entityValue?.ToString()),
                "isnotempty" => !string.IsNullOrEmpty(entityValue?.ToString()),

                // Regex operator
                "regex" => IsRegexMatch(entityValue?.ToString() ?? "", condition.Value?.ToString() ?? ""),
                "notregex" => !IsRegexMatch(entityValue?.ToString() ?? "", condition.Value?.ToString() ?? ""),

                // Date operators
                "today" => IsToday(entityValue),
                "yesterday" => IsYesterday(entityValue),
                "thisweek" => IsThisWeek(entityValue),
                "thismonth" => IsThisMonth(entityValue),
                "thisyear" => IsThisYear(entityValue),
                "lastndays" => IsLastNDays(entityValue, Convert.ToInt32(condition.Value ?? 0)),
                "lastnmonths" => IsLastNMonths(entityValue, Convert.ToInt32(condition.Value ?? 0)),

                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error evaluating condition: {Field} {Operation} {Value}",
                condition.Field, condition.Operation, condition.Value);
            return false;
        }
    }

  
    /// <summary>
    /// Validates rules recursively
    /// </summary>
    private void ValidateRules(SegmentRuleGroup ruleGroup, SegmentValidationResult result)
    {
        // Validate All rules
        if (ruleGroup.All?.Any() == true)
        {
            foreach (var rule in ruleGroup.All)
            {
                ValidateRule(rule, result);
            }
        }

        // Validate Any rules
        if (ruleGroup.Any?.Any() == true)
        {
            foreach (var rule in ruleGroup.Any)
            {
                ValidateRule(rule, result);
            }
        }

        // Validate Not rules
        if (ruleGroup.Not?.Any() == true)
        {
            foreach (var rule in ruleGroup.Not)
            {
                ValidateRule(rule, result);
            }
        }
    }

    /// <summary>
    /// Validates individual rule
    /// </summary>
    private void ValidateRule(SegmentRuleBase rule, SegmentValidationResult result)
    {
        if (rule is SegmentCondition condition)
        {
            // Validate field name
            if (string.IsNullOrWhiteSpace(condition.Field))
            {
                result.Errors.Add("Field name is required in conditions");
                return;
            }

            // Validate operator
            if (string.IsNullOrWhiteSpace(condition.Operation))
            {
                result.Errors.Add("Operator is required in conditions");
                return;
            }

            // Validate supported operators
            var supportedOperators = new[]
            {
                // Basic comparison
                "eq", "ne",
                // Numeric/date comparison
                "gt", "gte", "lt", "lte",
                // String operations
                "contains", "startswith", "endswith", "notcontains",
                // Collection operations
                "in", "notin",
                // Range operations
                "between", "notbetween",
                // Null/empty operations
                "isnull", "isnotnull", "isempty", "isnotempty",
                // Regex operations
                "regex", "notregex",
                // Date operations
                "today", "yesterday", "thisweek", "thismonth", "thisyear",
                "lastndays", "lastnmonths"
            };
            if (!supportedOperators.Contains(condition.Operation.ToLowerInvariant()))
            {
                result.Errors.Add($"Unsupported operator: {condition.Operation}. Supported operators: {string.Join(", ", supportedOperators)}");
            }
        }
        else if (rule is SegmentRuleGroup)
        {
            // Recursive validation for nested rules
            ValidateRules((SegmentRuleGroup)rule, result);
        }
    }

    /// <summary>
    /// Gets total count of entities for evaluation
    /// </summary>
    private async Task<int> GetTotalEntitiesCountAsync(
        List<OwnerType> targetOwnerTypes,
        CancellationToken cancellationToken)
    {
        var total = 0;

        if (targetOwnerTypes.Contains(OwnerType.Client))
        {
            total += await _context.Clients.CountAsync(cancellationToken);
        }

        if (targetOwnerTypes.Contains(OwnerType.Contact))
        {
            total += await _context.Contacts.CountAsync(cancellationToken);
        }

        return total;
    }

    #region Advanced Operator Helper Methods

    /// <summary>
    /// Compares two values for numeric/date comparison
    /// </summary>
    private static int CompareValues(object? value1, object? value2)
    {
        if (value1 == null && value2 == null) return 0;
        if (value1 == null) return -1;
        if (value2 == null) return 1;

        // Try numeric comparison
        if (decimal.TryParse(value1.ToString(), out var num1) && decimal.TryParse(value2.ToString(), out var num2))
            return num1.CompareTo(num2);

        // Try date comparison
        if (DateTime.TryParse(value1.ToString(), out var date1) && DateTime.TryParse(value2.ToString(), out var date2))
            return date1.CompareTo(date2);

        // String comparison
        return string.Compare(value1.ToString(), value2.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if a value is in a list (for "in" operator)
    /// </summary>
    private static bool IsInList(object? entityValue, object? listValue)
    {
        if (entityValue == null || listValue == null) return false;

        try
        {
            var entityStr = entityValue.ToString() ?? string.Empty;

            // Handle JSON array
            if (listValue.ToString()?.StartsWith("[") == true)
            {
                var list = JsonConvert.DeserializeObject<List<string>>(listValue.ToString() ?? "[]");
                return list?.Contains(entityStr) ?? false;
            }

            // Handle comma-separated string
            var values = listValue.ToString()?.Split(',').Select(v => v.Trim()).Where(v => !string.IsNullOrEmpty(v));
            return values?.Contains(entityStr) ?? false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a value is between two values (for "between" operator)
    /// </summary>
    private static bool IsBetween(object? entityValue, object? rangeValue)
    {
        if (entityValue == null || rangeValue == null) return false;

        try
        {
            var rangeStr = rangeValue.ToString() ?? string.Empty;
            var parts = rangeStr.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2) return false;

            var lower = parts[0].Trim();
            var upper = parts[1].Trim();

            // Handle numeric ranges
            if (decimal.TryParse(entityValue.ToString(), out var entityNum) &&
                decimal.TryParse(lower, out var lowerNum) &&
                decimal.TryParse(upper, out var upperNum))
            {
                return entityNum >= lowerNum && entityNum <= upperNum;
            }

            // Handle date ranges
            if (DateTime.TryParse(entityValue.ToString(), out var entityDate) &&
                DateTime.TryParse(lower, out var lowerDate) &&
                DateTime.TryParse(upper, out var upperDate))
            {
                return entityDate >= lowerDate && entityDate <= upperDate;
            }

            // Handle string ranges
            var entityStr = entityValue.ToString() ?? string.Empty;
            return string.Compare(entityStr, lower, StringComparison.OrdinalIgnoreCase) >= 0 &&
                   string.Compare(entityStr, upper, StringComparison.OrdinalIgnoreCase) <= 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a string matches a regex pattern
    /// </summary>
    private static bool IsRegexMatch(string input, string pattern)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pattern)) return false;

        try
        {
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a date is today
    /// </summary>
    private static bool IsToday(object? value)
    {
        if (value == null) return false;
        if (DateTime.TryParse(value.ToString(), out var date))
        {
            return date.Date == DateTime.Today;
        }
        return false;
    }

    /// <summary>
    /// Checks if a date is yesterday
    /// </summary>
    private static bool IsYesterday(object? value)
    {
        if (value == null) return false;
        if (DateTime.TryParse(value.ToString(), out var date))
        {
            return date.Date == DateTime.Today.AddDays(-1);
        }
        return false;
    }

    /// <summary>
    /// Checks if a date is within the current week
    /// </summary>
    private static bool IsThisWeek(object? value)
    {
        if (value == null) return false;
        if (DateTime.TryParse(value.ToString(), out var date))
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            return date.Date >= startOfWeek && date.Date < endOfWeek;
        }
        return false;
    }

    /// <summary>
    /// Checks if a date is within the current month
    /// </summary>
    private static bool IsThisMonth(object? value)
    {
        if (value == null) return false;
        if (DateTime.TryParse(value.ToString(), out var date))
        {
            var today = DateTime.Today;
            return date.Year == today.Year && date.Month == today.Month;
        }
        return false;
    }

    /// <summary>
    /// Checks if a date is within the current year
    /// </summary>
    private static bool IsThisYear(object? value)
    {
        if (value == null) return false;
        if (DateTime.TryParse(value.ToString(), out var date))
        {
            return date.Year == DateTime.Today.Year;
        }
        return false;
    }

    /// <summary>
    /// Checks if a date is within the last N days
    /// </summary>
    private static bool IsLastNDays(object? value, int days)
    {
        if (value == null || days <= 0) return false;
        if (DateTime.TryParse(value.ToString(), out var date))
        {
            var cutoff = DateTime.Today.AddDays(-days);
            return date.Date >= cutoff && date.Date <= DateTime.Today;
        }
        return false;
    }

    /// <summary>
    /// Checks if a date is within the last N months
    /// </summary>
    private static bool IsLastNMonths(object? value, int months)
    {
        if (value == null || months <= 0) return false;
        if (DateTime.TryParse(value.ToString(), out var date))
        {
            var cutoff = DateTime.Today.AddMonths(-months);
            return date.Date >= cutoff && date.Date <= DateTime.Today;
        }
        return false;
    }

    #endregion
}
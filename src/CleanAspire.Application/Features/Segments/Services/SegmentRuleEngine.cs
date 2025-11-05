using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using CleanAspire.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq.Expressions;

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
    /// Evaluates a condition against an entity
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
                "eq" => Equals(entityValue, condition.Value),
                "ne" => !Equals(entityValue, condition.Value),
                "contains" => entityValue?.ToString()?.Contains(condition.Value?.ToString() ?? "") ?? false,
                "startswith" => entityValue?.ToString()?.StartsWith(condition.Value?.ToString() ?? "") ?? false,
                "endswith" => entityValue?.ToString()?.EndsWith(condition.Value?.ToString() ?? "") ?? false,
                _ => false
            };
        }
        catch
        {
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
            var supportedOperators = new[] { "eq", "ne", "gt", "gte", "lt", "lte", "contains", "startswith", "endswith", "in" };
            if (!supportedOperators.Contains(condition.Operation.ToLowerInvariant()))
            {
                result.Errors.Add($"Unsupported operator: {condition.Operation}");
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
}
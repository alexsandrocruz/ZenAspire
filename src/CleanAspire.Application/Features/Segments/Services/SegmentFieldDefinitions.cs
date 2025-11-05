using CleanAspire.Domain.Enums;

namespace CleanAspire.Application.Features.Segments.Services;

/// <summary>
/// Defines available fields for segment rules
/// </summary>
public static class SegmentFieldDefinitions
{
    /// <summary>
    /// Available fields for Client entity
    /// </summary>
    public static readonly Dictionary<string, SegmentFieldInfo> ClientFields = new()
    {
        { "ClientType", new SegmentFieldInfo { DisplayName = "Client Type", Type = typeof(string), FieldType = FieldType.Text } },
        { "LifecycleStage", new SegmentFieldInfo { DisplayName = "Lifecycle Stage", Type = typeof(string), FieldType = FieldType.Select, Options = new List<string> { "Lead", "Prospect", "Client", "Former" } } },
        { "Industry", new SegmentFieldInfo { DisplayName = "Industry", Type = typeof(string), FieldType = FieldType.Text } },
        { "AnnualRevenue", new SegmentFieldInfo { DisplayName = "Annual Revenue", Type = typeof(decimal), FieldType = FieldType.Number } },
        { "EmployeeCount", new SegmentFieldInfo { DisplayName = "Employee Count", Type = typeof(int), FieldType = FieldType.Number } },
        { "Website", new SegmentFieldInfo { DisplayName = "Website", Type = typeof(string), FieldType = FieldType.Text } },
        { "TaxId", new SegmentFieldInfo { DisplayName = "Tax ID", Type = typeof(string), FieldType = FieldType.Text } },
        { "Created", new SegmentFieldInfo { DisplayName = "Created Date", Type = typeof(DateTime), FieldType = FieldType.Date } },
        { "LastInteractionDays", new SegmentFieldInfo { DisplayName = "Days Since Last Interaction", Type = typeof(int), FieldType = FieldType.Number } },
        { "TotalRevenue", new SegmentFieldInfo { DisplayName = "Total Revenue", Type = typeof(decimal), FieldType = FieldType.Number } },
        { "IsActive", new SegmentFieldInfo { DisplayName = "Is Active", Type = typeof(bool), FieldType = FieldType.Boolean } },
        { "HasContacts", new SegmentFieldInfo { DisplayName = "Has Contacts", Type = typeof(bool), FieldType = FieldType.Boolean } },
        { "ContactCount", new SegmentFieldInfo { DisplayName = "Contact Count", Type = typeof(int), FieldType = FieldType.Number } }
    };

    /// <summary>
    /// Available fields for Contact entity
    /// </summary>
    public static readonly Dictionary<string, SegmentFieldInfo> ContactFields = new()
    {
        { "FirstName", new SegmentFieldInfo { DisplayName = "First Name", Type = typeof(string), FieldType = FieldType.Text } },
        { "LastName", new SegmentFieldInfo { DisplayName = "Last Name", Type = typeof(string), FieldType = FieldType.Text } },
        { "Email", new SegmentFieldInfo { DisplayName = "Email", Type = typeof(string), FieldType = FieldType.Text } },
        { "Phone", new SegmentFieldInfo { DisplayName = "Phone", Type = typeof(string), FieldType = FieldType.Text } },
        { "Mobile", new SegmentFieldInfo { DisplayName = "Mobile", Type = typeof(string), FieldType = FieldType.Text } },
        { "Position", new SegmentFieldInfo { DisplayName = "Position", Type = typeof(string), FieldType = FieldType.Text } },
        { "Department", new SegmentFieldInfo { DisplayName = "Department", Type = typeof(string), FieldType = FieldType.Text } },
        { "IsPrimary", new SegmentFieldInfo { DisplayName = "Is Primary", Type = typeof(bool), FieldType = FieldType.Boolean } },
        { "IsDecisionMaker", new SegmentFieldInfo { DisplayName = "Is Decision Maker", Type = typeof(bool), FieldType = FieldType.Boolean } },
        { "Created", new SegmentFieldInfo { DisplayName = "Created Date", Type = typeof(DateTime), FieldType = FieldType.Date } },
        { "LastInteractionDays", new SegmentFieldInfo { DisplayName = "Days Since Last Interaction", Type = typeof(int), FieldType = FieldType.Number } }
    };

    /// <summary>
    /// Gets all available fields by entity type
    /// </summary>
    public static Dictionary<string, SegmentFieldInfo> GetFieldsForEntityType(OwnerType ownerType)
    {
        return ownerType switch
        {
            OwnerType.Client => ClientFields,
            OwnerType.Contact => ContactFields,
            _ => new Dictionary<string, SegmentFieldInfo>()
        };
    }

    /// <summary>
    /// Validates field name availability
    /// </summary>
    public static bool IsValidField(OwnerType ownerType, string fieldName)
    {
        var fields = GetFieldsForEntityType(ownerType);
        return fields.ContainsKey(fieldName);
    }

    /// <summary>
    /// Gets field information
    /// </summary>
    public static SegmentFieldInfo? GetFieldInfo(OwnerType ownerType, string fieldName)
    {
        var fields = GetFieldsForEntityType(ownerType);
        return fields.TryGetValue(fieldName, out var fieldInfo) ? fieldInfo : null;
    }
}

/// <summary>
/// Information about a segment field
/// </summary>
public class SegmentFieldInfo
{
    public string DisplayName { get; set; } = string.Empty;
    public Type Type { get; set; } = typeof(string);
    public FieldType FieldType { get; set; } = FieldType.Text;
    public List<string> Options { get; set; } = new();
    public string? Description { get; set; }
    public bool IsSearchable { get; set; } = true;
    public bool IsSortable { get; set; } = true;
}

/// <summary>
/// Field types for segment rules
/// </summary>
public enum FieldType
{
    Text,
    Number,
    Date,
    Boolean,
    Select,
    MultiSelect
}
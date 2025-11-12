namespace CleanAspire.Domain.Enums;

/// <summary>
/// Operators supported in segment rule definitions
/// </summary>
public enum SegmentOperator
{
    /// <summary>
    /// Equal to (==)
    /// </summary>
    Equal = 1,

    /// <summary>
    /// Not equal to (!=)
    /// </summary>
    NotEqual = 2,

    /// <summary>
    /// Greater than (>)
    /// </summary>
    GreaterThan = 3,

    /// <summary>
    /// Greater than or equal to (>=)
    /// </summary>
    GreaterThanOrEqual = 4,

    /// <summary>
    /// Less than (<)
    /// </summary>
    LessThan = 5,

    /// <summary>
    /// Less than or equal to (<=)
    /// </summary>
    LessThanOrEqual = 6,

    /// <summary>
    /// Contains (string contains)
    /// </summary>
    Contains = 7,

    /// <summary>
    /// Starts with (string starts with)
    /// </summary>
    StartsWith = 8,

    /// <summary>
    /// Ends with (string ends with)
    /// </summary>
    EndsWith = 9,

    /// <summary>
    /// In (value is in list)
    /// </summary>
    In = 10,

    /// <summary>
    /// Not in (value is not in list)
    /// </summary>
    NotIn = 11,

    /// <summary>
    /// Days ago (date is X days ago)
    /// </summary>
    DaysAgo = 12,

    /// <summary>
    /// Within last days (date is within last X days)
    /// </summary>
    WithinLastDays = 13,

    /// <summary>
    /// Before date (date is before specified date)
    /// </summary>
    Before = 14,

    /// <summary>
    /// After date (date is after specified date)
    /// </summary>
    After = 15,

    /// <summary>
    /// Has tag (entity has specified tag)
    /// </summary>
    HasTag = 16,

    /// <summary>
    /// Is null (field is null)
    /// </summary>
    IsNull = 17,

    /// <summary>
    /// Is not null (field is not null)
    /// </summary>
    IsNotNull = 18,

    /// <summary>
    /// Is empty (string is empty)
    /// </summary>
    IsEmpty = 19,

    /// <summary>
    /// Is not empty (string is not empty)
    /// </summary>
    IsNotEmpty = 20
}

/// <summary>
/// Logical operators for combining segment rules
/// </summary>
public enum SegmentLogicalOperator
{
    /// <summary>
    /// All conditions must be true (AND)
    /// </summary>
    All = 1,

    /// <summary>
    /// Any condition must be true (OR)
    /// </summary>
    Any = 2,

    /// <summary>
    /// No conditions must be true (NOT)
    /// </summary>
    Not = 3
}
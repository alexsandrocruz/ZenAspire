// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Application.Features.Activities.Caching;

/// <summary>
/// Defines cache keys for Activity-related operations.
/// Provides consistent cache key generation across the application.
/// </summary>
public static class ActivityCacheKey
{
    /// <summary>
    /// Cache key prefix for all activity-related cache entries
    /// </summary>
    private const string Prefix = "activities";

    /// <summary>
    /// Tag used for cache invalidation of activity entries
    /// </summary>
    public const string Tag = "activities";

    /// <summary>
    /// Gets the cache key for a specific activity by ID
    /// </summary>
    /// <param name="id">The activity ID</param>
    /// <returns>Cache key string</returns>
    public static string GetById(string id) => $"{Prefix}_{id}";

    /// <summary>
    /// Gets the cache key for activities list with specific parameters
    /// </summary>
    /// <param name="parameters">Query parameters for filtering/paging</param>
    /// <returns>Cache key string</returns>
    public static string GetList(string parameters) => $"{Prefix}_list_{parameters}";

    /// <summary>
    /// Gets the cache key for activities assigned to a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="parameters">Additional query parameters</param>
    /// <returns>Cache key string</returns>
    public static string GetByUser(string userId, string parameters = "") =>
        $"{Prefix}_user_{userId}_{parameters}";

    /// <summary>
    /// Gets the cache key for activities regarding a specific entity
    /// </summary>
    /// <param name="regardingType">The entity type (Client, Contact, etc.)</param>
    /// <param name="regardingId">The entity ID</param>
    /// <returns>Cache key string</returns>
    public static string GetByRegarding(string regardingType, string regardingId) =>
        $"{Prefix}_regarding_{regardingType}_{regardingId}";

    /// <summary>
    /// Gets the cache key for overdue activities
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <returns>Cache key string</returns>
    public static string GetOverdue(string? userId = null) =>
        string.IsNullOrEmpty(userId)
            ? $"{Prefix}_overdue"
            : $"{Prefix}_overdue_{userId}";

    /// <summary>
    /// Gets the cache key for activity statistics/counts
    /// </summary>
    /// <param name="parameters">Statistics parameters</param>
    /// <returns>Cache key string</returns>
    public static string GetStats(string parameters = "") =>
        $"{Prefix}_stats_{parameters}";
}

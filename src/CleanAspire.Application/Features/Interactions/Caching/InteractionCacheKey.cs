// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CleanAspire.Application.Features.Interactions.Caching;

/// <summary>
/// Defines cache keys for Interaction-related operations.
/// Provides consistent cache key generation across the application.
/// </summary>
public static class InteractionCacheKey
{
    /// <summary>
    /// Cache key prefix for all interaction-related cache entries
    /// </summary>
    private const string Prefix = "interactions";

    /// <summary>
    /// Tag used for cache invalidation of interaction entries
    /// </summary>
    public const string Tag = "interactions";

    /// <summary>
    /// Gets the cache key for a specific interaction by ID
    /// </summary>
    /// <param name="id">The interaction ID</param>
    /// <returns>Cache key string</returns>
    public static string GetById(string id) => $"{Prefix}_{id}";

    /// <summary>
    /// Gets the cache key for interactions list with specific parameters
    /// </summary>
    /// <param name="parameters">Query parameters for filtering/paging</param>
    /// <returns>Cache key string</returns>
    public static string GetList(string parameters) => $"{Prefix}_list_{parameters}";

    /// <summary>
    /// Gets the cache key for interactions by owner (Client, Contact, etc.)
    /// </summary>
    /// <param name="ownerType">The owner entity type</param>
    /// <param name="ownerId">The owner entity ID</param>
    /// <param name="parameters">Additional query parameters</param>
    /// <returns>Cache key string</returns>
    public static string GetByOwner(string ownerType, string ownerId, string parameters = "") =>
        $"{Prefix}_owner_{ownerType}_{ownerId}_{parameters}";

    /// <summary>
    /// Gets the cache key for interactions by type
    /// </summary>
    /// <param name="type">The interaction type</param>
    /// <param name="parameters">Additional query parameters</param>
    /// <returns>Cache key string</returns>
    public static string GetByType(string type, string parameters = "") =>
        $"{Prefix}_type_{type}_{parameters}";

    /// <summary>
    /// Gets the cache key for interactions by direction
    /// </summary>
    /// <param name="direction">The interaction direction (Inbound/Outbound/Internal)</param>
    /// <param name="parameters">Additional query parameters</param>
    /// <returns>Cache key string</returns>
    public static string GetByDirection(string direction, string parameters = "") =>
        $"{Prefix}_direction_{direction}_{parameters}";

    /// <summary>
    /// Gets the cache key for interactions by date range
    /// </summary>
    /// <param name="startDate">Start date in ISO format</param>
    /// <param name="endDate">End date in ISO format</param>
    /// <param name="parameters">Additional query parameters</param>
    /// <returns>Cache key string</returns>
    public static string GetByDateRange(string startDate, string endDate, string parameters = "") =>
        $"{Prefix}_daterange_{startDate}_{endDate}_{parameters}";

    /// <summary>
    /// Gets the cache key for interaction statistics/counts
    /// </summary>
    /// <param name="parameters">Statistics parameters</param>
    /// <returns>Cache key string</returns>
    public static string GetStats(string parameters = "") =>
        $"{Prefix}_stats_{parameters}";

    /// <summary>
    /// Gets the cache key for recent interactions
    /// </summary>
    /// <param name="count">Number of recent interactions</param>
    /// <param name="parameters">Additional query parameters</param>
    /// <returns>Cache key string</returns>
    public static string GetRecent(int count, string parameters = "") =>
        $"{Prefix}_recent_{count}_{parameters}";
}

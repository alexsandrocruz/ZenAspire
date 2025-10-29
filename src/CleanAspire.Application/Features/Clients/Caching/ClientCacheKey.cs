namespace CleanAspire.Application.Features.Clients.Caching;

/// <summary>
/// Static class containing cache keys and configuration for Client-related caching.
/// Provides centralized cache key management for performance optimization.
/// </summary>
public static class ClientCacheKey
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(2);
    
    /// <summary>
    /// Default cache duration for client data.
    /// </summary>
    public static TimeSpan? Duration => DefaultDuration;
    
    /// <summary>
    /// Cache key for all clients list.
    /// </summary>
    public static string GetAllCacheKey => "all-clients";
    
    /// <summary>
    /// Generates cache key for paginated clients query.
    /// </summary>
    /// <param name="parameters">Query parameters for uniqueness</param>
    /// <returns>Unique cache key for the pagination query</returns>
    public static string GetPaginationCacheKey(string parameters) => $"clients-pagination-{parameters}";
    
    /// <summary>
    /// Generates cache key for a specific client by ID.
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>Unique cache key for the client</returns>
    public static string GetByIdCacheKey(string id) => $"client-by-id-{id}";
    
    /// <summary>
    /// Generates cache key for clients export query.
    /// </summary>
    /// <param name="parameters">Export parameters for uniqueness</param>
    /// <returns>Unique cache key for the export query</returns>
    public static string GetExportCacheKey(string parameters) => $"clients-export-{parameters}";
    
    private static CancellationTokenSource _tokenSource = new();
    
    /// <summary>
    /// Shared expiry token for cache invalidation.
    /// </summary>
    public static CancellationToken SharedExpiryToken => _tokenSource.Token;
    
    /// <summary>
    /// Refreshes the shared expiry token, invalidating all cached client data.
    /// </summary>
    public static void Refresh() => SharedExpiryTokenSource();
    
    private static void SharedExpiryTokenSource()
    {
        _tokenSource.Cancel();
        _tokenSource = new CancellationTokenSource(RefreshInterval);
    }
}
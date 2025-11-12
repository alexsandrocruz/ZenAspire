namespace CleanAspire.Application.Features.Contacts.Caching;

/// <summary>
/// Static class containing cache keys and configuration for Contact-related caching.
/// Provides centralized cache key management for performance optimization.
/// </summary>
public static class ContactCacheKey
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(1);
    
    /// <summary>
    /// Default cache duration for contact data.
    /// </summary>
    public static TimeSpan? Duration => DefaultDuration;
    
    /// <summary>
    /// Cache key for all contacts list.
    /// </summary>
    public static string GetAllCacheKey => "all-contacts";
    
    /// <summary>
    /// Generates cache key for paginated contacts query.
    /// </summary>
    /// <param name="parameters">Query parameters for uniqueness</param>
    /// <returns>Unique cache key for the pagination query</returns>
    public static string GetPaginationCacheKey(string parameters) => $"contacts-pagination-{parameters}";
    
    /// <summary>
    /// Generates cache key for a specific contact by ID.
    /// </summary>
    /// <param name="id">Contact ID</param>
    /// <returns>Unique cache key for the contact</returns>
    public static string GetByIdCacheKey(string id) => $"contact-by-id-{id}";
    
    /// <summary>
    /// Generates cache key for contacts by client ID.
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <returns>Unique cache key for client's contacts</returns>
    public static string GetByClientIdCacheKey(string clientId) => $"contacts-by-client-{clientId}";
    
    /// <summary>
    /// Generates cache key for contacts export query.
    /// </summary>
    /// <param name="parameters">Export parameters for uniqueness</param>
    /// <returns>Unique cache key for the export query</returns>
    public static string GetExportCacheKey(string parameters) => $"contacts-export-{parameters}";
    
    private static CancellationTokenSource _tokenSource = new();
    
    /// <summary>
    /// Shared expiry token for cache invalidation.
    /// </summary>
    public static CancellationToken SharedExpiryToken => _tokenSource.Token;
    
    /// <summary>
    /// Refreshes the shared expiry token, invalidating all cached contact data.
    /// </summary>
    public static void Refresh() => SharedExpiryTokenSource();
    
    private static void SharedExpiryTokenSource()
    {
        _tokenSource.Cancel();
        _tokenSource = new CancellationTokenSource(RefreshInterval);
    }
}
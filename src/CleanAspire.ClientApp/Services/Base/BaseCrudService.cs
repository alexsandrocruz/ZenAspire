using Microsoft.AspNetCore.Components;
using Microsoft.Kiota.Abstractions;

namespace CleanAspire.ClientApp.Services.Base;

/// <summary>
/// Base service for CRUD operations using API client.
/// Temporary implementation until API client models are regenerated.
/// </summary>
/// <typeparam name="TDto">The DTO type</typeparam>
/// <typeparam name="TCreateRequest">The create request type</typeparam>
/// <typeparam name="TKey">The key type</typeparam>
public abstract class BaseCrudService<TDto, TCreateRequest, TKey>
{
    protected readonly HttpClient _httpClient;
    protected readonly IAuthTokenProvider _tokenProvider;
    protected readonly LazyAssemblyLoader _lazyAssemblyLoader;

    protected BaseCrudService(HttpClient httpClient, IAuthTokenProvider tokenProvider, LazyAssemblyLoader lazyAssemblyLoader)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _lazyAssemblyLoader = lazyAssemblyLoader;
    }

    protected abstract string GetApiEndpoint();

    protected virtual Task<object> GetClientAsync() => throw new NotImplementedException("GetClientAsync must be implemented by derived classes");
}

/// <summary>
/// Temporary interface for auth token provider
/// </summary>
public interface IAuthTokenProvider
{
    Task<string?> GetTokenAsync();
}

/// <summary>
/// Temporary class for lazy assembly loading
/// </summary>
public class LazyAssemblyLoader
{
    public LazyAssemblyLoader()
    {
    }
}
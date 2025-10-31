using System.Net.Http.Json;
using CleanAspire.ClientApp.DTOs;

namespace CleanAspire.ClientApp.Services.Clients;

/// <summary>
/// Service proxy for managing client operations.
/// Temporary implementation with stub methods for compilation.
/// </summary>
public class ClientServiceProxy
{
    private readonly HttpClient _httpClient;

    public ClientServiceProxy(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get clients with pagination
    /// </summary>
    public async Task<PaginatedResult<ClientDto>?> GetClientsWithPaginationAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            var request = new
            {
                Keywords = searchTerm ?? string.Empty,
                PageNumber = pageNumber - 1, // API uses zero-based indexing (0 = first page)
                PageSize = pageSize,
                OrderBy = "Name",
                SortDirection = "Ascending"
            };

            var response = await _httpClient.PostAsJsonAsync("/api/clients/pagination", request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error getting paginated clients:");
                Console.WriteLine($"   Status: {response.StatusCode} ({(int)response.StatusCode})");
                Console.WriteLine($"   URL: {response.RequestMessage?.RequestUri}");
                Console.WriteLine($"   Error: {errorContent}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<PaginatedResult<ClientDto>>();
            Console.WriteLine($"✅ Got {result?.Items?.Count() ?? 0} clients successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting paginated clients: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get client by ID
    /// </summary>
    public async Task<ClientDto?> GetClientByIdAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/clients/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting client by ID: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ClientDto>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting client by ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create a new client
    /// </summary>
    public async Task<ClientDto?> CreateClientAsync(ClientDto clientDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/clients", clientDto);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error creating client: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ClientDto>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating client: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    public async Task<bool> UpdateClientAsync(string id, ClientDto clientDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("/api/clients", clientDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating client: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete a client
    /// </summary>
    public async Task<bool> DeleteClientAsync(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/clients/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting client: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get clients by type
    /// </summary>
    public async Task<IEnumerable<ClientDto>?> GetClientsByTypeAsync(int clientType)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/clients?type={clientType}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting clients by type: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ClientDto>>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting clients by type: {ex.Message}");
            return null;
        }
    }
}
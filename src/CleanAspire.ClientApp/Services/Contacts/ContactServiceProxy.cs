using System.Net.Http.Json;
using CleanAspire.ClientApp.DTOs;

namespace CleanAspire.ClientApp.Services.Contacts;

/// <summary>
/// Service proxy for managing contact operations.
/// Temporary implementation with stub methods for compilation.
/// </summary>
public class ContactServiceProxy
{
    private readonly HttpClient _httpClient;

    public ContactServiceProxy(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get contacts with pagination
    /// </summary>
    public async Task<PaginatedResult<ContactDto>?> GetContactsWithPaginationAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? clientId = null)
    {
        try
        {
            var queryString = $"pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            }
            if (!string.IsNullOrEmpty(clientId))
            {
                queryString += $"&clientId={Uri.EscapeDataString(clientId)}";
            }

            var response = await _httpClient.GetAsync($"/api/contacts/pagination?{queryString}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting paginated contacts: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<PaginatedResult<ContactDto>>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting paginated contacts: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get contact by ID
    /// </summary>
    public async Task<ContactDto?> GetContactByIdAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/contacts/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting contact by ID: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ContactDto>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting contact by ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get contacts by client ID
    /// </summary>
    public async Task<List<ContactDto>?> GetContactsByClientIdAsync(string clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/contacts/by-client/{clientId}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting contacts by client ID: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<List<ContactDto>>();
            return result ?? new List<ContactDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting contacts by client ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create a new contact
    /// </summary>
    public async Task<ContactDto?> CreateContactAsync(ContactDto contactDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/contacts", contactDto);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error creating contact: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ContactDto>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating contact: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update an existing contact
    /// </summary>
    public async Task<bool> UpdateContactAsync(string id, ContactDto contactDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("/api/contacts", contactDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating contact: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete a contact
    /// </summary>
    public async Task<bool> DeleteContactAsync(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/contacts/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting contact: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get contacts by type
    /// </summary>
    public async Task<IEnumerable<ContactDto>?> GetContactsByTypeAsync(int contactType)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/contacts?type={contactType}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting contacts by type: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ContactDto>>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting contacts by type: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get standalone contacts (contacts without associated client)
    /// </summary>
    public async Task<IEnumerable<ContactDto>?> GetStandaloneContactsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/contacts?standalone=true");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting standalone contacts: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ContactDto>>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting standalone contacts: {ex.Message}");
            return null;
        }
    }
}
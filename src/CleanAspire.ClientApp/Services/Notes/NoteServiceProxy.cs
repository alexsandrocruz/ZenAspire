using System.Net.Http.Json;
using CleanAspire.ClientApp.DTOs;

namespace CleanAspire.ClientApp.Services.Notes;

/// <summary>
/// Service proxy for managing note operations.
/// </summary>
public class NoteServiceProxy
{
    private readonly HttpClient _httpClient;

    public NoteServiceProxy(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get notes by owner (client, contact, etc.)
    /// </summary>
    public async Task<IEnumerable<NoteDto>?> GetNotesByOwnerAsync(OwnerType ownerType, Guid ownerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/crm/notes/{ownerType}/{ownerId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error getting notes by owner:");
                Console.WriteLine($"   Status: {response.StatusCode} ({(int)response.StatusCode})");
                Console.WriteLine($"   URL: {response.RequestMessage?.RequestUri}");
                Console.WriteLine($"   Error: {errorContent}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<NoteDto>>();
            Console.WriteLine($"✅ Got {result?.Count() ?? 0} notes successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting notes by owner: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get note by ID
    /// </summary>
    public async Task<NoteDto?> GetNoteByIdAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/crm/notes/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error getting note by ID: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<NoteDto>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting note by ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create a new note
    /// </summary>
    public async Task<NoteDto?> CreateNoteAsync(NoteRequest noteRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/crm/notes", noteRequest);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error creating note: {response.StatusCode} - {errorContent}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<NoteDto>();
            Console.WriteLine($"✅ Note created successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating note: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update an existing note
    /// </summary>
    public async Task<bool> UpdateNoteAsync(string id, NoteRequest noteRequest)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/crm/notes", noteRequest);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error updating note: {response.StatusCode} - {errorContent}");
                return false;
            }

            Console.WriteLine($"✅ Note updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating note: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete a note
    /// </summary>
    public async Task<bool> DeleteNoteAsync(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/crm/notes/{id}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error deleting note: {response.StatusCode}");
                return false;
            }

            Console.WriteLine($"✅ Note deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting note: {ex.Message}");
            return false;
        }
    }
}
// This class acts as a service proxy for managing customers.
// It provides methods for CRUD operations using HttpClient directly.
// Note: This is a simplified version until the OpenAPI client is generated with Customer endpoints.

using System.Net.Http.Json;

namespace CleanAspire.ClientApp.Services.Customers;

/// <summary>
/// Simple DTO for Customer data transfer between client and API.
/// </summary>
public class CustomerDto
{
    public string? Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Simple pagination query parameters.
/// </summary>
public class CustomersWithPaginationQuery
{
    public string Keywords { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 15;
    public string OrderBy { get; set; } = "Id";
    public string SortDirection { get; set; } = "Descending";
}

/// <summary>
/// Simple paginated result wrapper.
/// </summary>
public class PaginatedResultOfCustomerDto
{
    public List<CustomerDto> Items { get; set; } = new();
    public long TotalItems { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// Simple command for creating customers.
/// </summary>
public class CreateCustomerCommand
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Simple command for updating customers.
/// </summary>
public class UpdateCustomerCommand
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Service proxy for managing customer operations through API calls.
/// </summary>
public class CustomerServiceProxy
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "/customers";

    /// <summary>
    /// Initializes a new instance of the CustomerServiceProxy.
    /// </summary>
    public CustomerServiceProxy(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets a paginated list of customers.
    /// </summary>
    public async Task<PaginatedResultOfCustomerDto> GetPaginatedCustomersAsync(CustomersWithPaginationQuery query)
    {
        try
        {
            var queryString = $"?keywords={Uri.EscapeDataString(query.Keywords)}&pageNumber={query.PageNumber}&pageSize={query.PageSize}&orderBy={query.OrderBy}&sortDirection={query.SortDirection}";
            var response = await _httpClient.GetFromJsonAsync<PaginatedResultOfCustomerDto>($"{BaseUrl}{queryString}");
            return response ?? new PaginatedResultOfCustomerDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting customers: {ex.Message}");
            return new PaginatedResultOfCustomerDto();
        }
    }

    /// <summary>
    /// Gets a customer by ID.
    /// </summary>
    public async Task<CustomerDto?> GetCustomerByIdAsync(string customerId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CustomerDto>($"{BaseUrl}/{customerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting customer: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    public async Task<CustomerDto?> CreateCustomerAsync(CreateCustomerCommand command)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, command);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CustomerDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating customer: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    public async Task<bool> UpdateCustomerAsync(UpdateCustomerCommand command)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{command.Id}", command);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating customer: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes a customer.
    /// </summary>
    public async Task<bool> DeleteCustomerAsync(string customerId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{customerId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting customer: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Exports customers to CSV.
    /// </summary>
    public async Task<Stream?> ExportCustomersAsync(string keywords = "")
    {
        try
        {
            var queryString = string.IsNullOrEmpty(keywords) ? "" : $"?keywords={Uri.EscapeDataString(keywords)}";
            var response = await _httpClient.GetAsync($"{BaseUrl}/export{queryString}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting customers: {ex.Message}");
            return null;
        }
    }
}
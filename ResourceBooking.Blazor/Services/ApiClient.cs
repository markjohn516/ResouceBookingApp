using System.Net.Http.Json;
using ResourceBooking.Core.DTOs;

namespace ResourceBooking.Blazor.Services;

public interface IApiClient
{
    // Employees
    Task<List<EmployeeDto>> GetEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto);
    Task UpdateEmployeeAsync(int id, UpdateEmployeeDto dto);
    Task DeleteEmployeeAsync(int id);

    // Resources
    Task<List<ResourceDto>> GetResourcesAsync();
    Task<List<ResourceDto>> GetAllResourcesAsync();
    Task<List<ResourceDto>> GetActiveResourcesAsync();
    Task<ResourceDto?> GetResourceAsync(int id);
    Task<ResourceDto> CreateResourceAsync(CreateResourceDto dto);
    Task UpdateResourceAsync(int id, UpdateResourceDto dto);
    Task DisableResourceAsync(int id);
    Task EnableResourceAsync(int id);

    // Bookings
    Task<List<BookingDto>> GetBookingsAsync();
    Task<BookingDto?> GetBookingAsync(int id);
    Task<List<BookingDto>> GetResourceBookingsAsync(int resourceId);
    Task<List<BookingDto>> GetEmployeeBookingsAsync(int employeeId);
    Task<BookingConflictResponse> CheckConflictAsync(CreateBookingDto dto);
    Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);
    Task DeleteBookingAsync(int id);
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private const string BaseUrl = "https://localhost:7000";

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    // Employees
    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<EmployeeDto>>("/api/employees") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees");
            throw;
        }
    }

    public async Task<EmployeeDto?> GetEmployeeAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<EmployeeDto>($"/api/employees/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee {Id}", id);
            throw;
        }
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/employees", dto);
            var result = await response.Content.ReadFromJsonAsync<EmployeeDto>();
            if (result == null)
                throw new InvalidOperationException("Failed to deserialize employee response");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            throw;
        }
    }

    public async Task UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
    {
        try
        {
            await _httpClient.PutAsJsonAsync($"/api/employees/{id}", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {Id}", id);
            throw;
        }
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        try
        {
            await _httpClient.DeleteAsync($"/api/employees/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {Id}", id);
            throw;
        }
    }

    // Resources
    public async Task<List<ResourceDto>> GetResourcesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ResourceDto>>("/api/resources") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resources");
            throw;
        }
    }

    public async Task<List<ResourceDto>> GetAllResourcesAsync()
    {
        return await GetResourcesAsync();
    }

    public async Task<List<ResourceDto>> GetActiveResourcesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ResourceDto>>("/api/resources/active") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active resources");
            throw;
        }
    }

    public async Task<ResourceDto?> GetResourceAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ResourceDto>($"/api/resources/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource {Id}", id);
            throw;
        }
    }

    public async Task<ResourceDto> CreateResourceAsync(CreateResourceDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/resources", dto);
            var result = await response.Content.ReadFromJsonAsync<ResourceDto>();
            if (result == null)
                throw new InvalidOperationException("Failed to deserialize resource response");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource");
            throw;
        }
    }

    public async Task UpdateResourceAsync(int id, UpdateResourceDto dto)
    {
        try
        {
            await _httpClient.PutAsJsonAsync($"/api/resources/{id}", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource {Id}", id);
            throw;
        }
    }

    public async Task DisableResourceAsync(int id)
    {
        try
        {
            await _httpClient.PostAsync($"/api/resources/{id}/disable", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling resource {Id}", id);
            throw;
        }
    }

    public async Task EnableResourceAsync(int id)
    {
        try
        {
            await _httpClient.PostAsync($"/api/resources/{id}/enable", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling resource {Id}", id);
            throw;
        }
    }

    // Bookings
    public async Task<List<BookingDto>> GetBookingsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<BookingDto>>("/api/bookings") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings");
            throw;
        }
    }

    public async Task<BookingDto?> GetBookingAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BookingDto>($"/api/bookings/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking {Id}", id);
            throw;
        }
    }

    public async Task<List<BookingDto>> GetResourceBookingsAsync(int resourceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<BookingDto>>($"/api/bookings/resource/{resourceId}") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for resource {ResourceId}", resourceId);
            throw;
        }
    }

    public async Task<List<BookingDto>> GetEmployeeBookingsAsync(int employeeId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<BookingDto>>($"/api/bookings/employee/{employeeId}") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<BookingConflictResponse> CheckConflictAsync(CreateBookingDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/bookings/check-conflict", dto);
            var result = await response.Content.ReadFromJsonAsync<BookingConflictResponse>();
            if (result == null)
                throw new InvalidOperationException("Failed to deserialize conflict response");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for conflicts");
            throw;
        }
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/bookings", dto);
            var result = await response.Content.ReadFromJsonAsync<BookingDto>();
            if (result == null)
                throw new InvalidOperationException("Failed to deserialize booking response");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            throw;
        }
    }

    public async Task DeleteBookingAsync(int id)
    {
        try
        {
            await _httpClient.DeleteAsync($"/api/bookings/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking {Id}", id);
            throw;
        }
    }
}

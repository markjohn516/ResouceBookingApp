using ResourceBooking.Api.Repositories;
using ResourceBooking.Core.DTOs;
using ResourceBooking.Core.Models;

namespace ResourceBooking.Api.Services;

public interface IEmployeeService
{
    Task<EmployeeDto?> GetEmployeeAsync(int id);
    Task<List<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createDto);
    Task UpdateEmployeeAsync(int id, UpdateEmployeeDto updateDto);
    Task DeleteEmployeeAsync(int id);
}

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeDto?> GetEmployeeAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllActiveAsync();
        return employees.Select(MapToDto).ToList();
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.FirstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(createDto.LastName))
            throw new ArgumentException("Last name is required.");

        var employee = new Employee
        {
            FirstName = createDto.FirstName.Trim(),
            LastName = createDto.LastName.Trim()
        };

        await _employeeRepository.AddAsync(employee);
        return MapToDto(employee);
    }

    public async Task UpdateEmployeeAsync(int id, UpdateEmployeeDto updateDto)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
            throw new InvalidOperationException($"Employee with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(updateDto.FirstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(updateDto.LastName))
            throw new ArgumentException("Last name is required.");

        employee.FirstName = updateDto.FirstName.Trim();
        employee.LastName = updateDto.LastName.Trim();

        await _employeeRepository.UpdateAsync(employee);
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        await _employeeRepository.DeleteAsync(id);
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName
        };
    }
}

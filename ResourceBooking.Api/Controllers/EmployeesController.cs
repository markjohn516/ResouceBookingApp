using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Api.Services;
using ResourceBooking.Core.DTOs;

namespace ResourceBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
    {
        try
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees");
            return StatusCode(500, "An error occurred while retrieving employees.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeAsync(id);
            if (employee == null)
                return NotFound("Employee not found.");

            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the employee.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto createDto)
    {
        try
        {
            var employee = await _employeeService.CreateEmployeeAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, "An error occurred while creating the employee.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto updateDto)
    {
        try
        {
            await _employeeService.UpdateEmployeeAsync(id, updateDto);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {Id}", id);
            return StatusCode(500, "An error occurred while updating the employee.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {Id}", id);
            return StatusCode(500, "An error occurred while deleting the employee.");
        }
    }
}

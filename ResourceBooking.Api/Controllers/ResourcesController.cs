using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Api.Services;
using ResourceBooking.Core.DTOs;

namespace ResourceBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(IResourceService resourceService, ILogger<ResourcesController> logger)
    {
        _resourceService = resourceService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ResourceDto>>> GetAll()
    {
        try
        {
            var resources = await _resourceService.GetAllResourcesAsync();
            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resources");
            return StatusCode(500, "An error occurred while retrieving resources.");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<ResourceDto>>> GetActive()
    {
        try
        {
            var resources = await _resourceService.GetActiveResourcesAsync();
            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active resources");
            return StatusCode(500, "An error occurred while retrieving active resources.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceDto>> GetById(int id)
    {
        try
        {
            var resource = await _resourceService.GetResourceAsync(id);
            if (resource == null)
                return NotFound("Resource not found.");

            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the resource.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ResourceDto>> Create([FromBody] CreateResourceDto createDto)
    {
        try
        {
            var resource = await _resourceService.CreateResourceAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource");
            return StatusCode(500, "An error occurred while creating the resource.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResourceDto updateDto)
    {
        try
        {
            await _resourceService.UpdateResourceAsync(id, updateDto);
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
            _logger.LogError(ex, "Error updating resource {Id}", id);
            return StatusCode(500, "An error occurred while updating the resource.");
        }
    }

    [HttpPost("{id}/disable")]
    public async Task<IActionResult> Disable(int id)
    {
        try
        {
            await _resourceService.DisableResourceAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling resource {Id}", id);
            return StatusCode(500, "An error occurred while disabling the resource.");
        }
    }

    [HttpPost("{id}/enable")]
    public async Task<IActionResult> Enable(int id)
    {
        try
        {
            await _resourceService.EnableResourceAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling resource {Id}", id);
            return StatusCode(500, "An error occurred while enabling the resource.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _resourceService.GetResourceAsync(id);
            // In a real scenario, you might want to prevent deletion if bookings exist
            // For now, we'll just soft delete by disabling
            await _resourceService.DisableResourceAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource {Id}", id);
            return StatusCode(500, "An error occurred while deleting the resource.");
        }
    }
}

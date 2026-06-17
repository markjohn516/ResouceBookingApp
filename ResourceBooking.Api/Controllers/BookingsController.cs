using Microsoft.AspNetCore.Mvc;
using ResourceBooking.Api.Services;
using ResourceBooking.Core.DTOs;

namespace ResourceBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookingDto>>> GetAll()
    {
        try
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings");
            return StatusCode(500, "An error occurred while retrieving bookings.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> GetById(int id)
    {
        try
        {
            var booking = await _bookingService.GetBookingAsync(id);
            if (booking == null)
                return NotFound("Booking not found.");

            return Ok(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the booking.");
        }
    }

    [HttpGet("resource/{resourceId}")]
    public async Task<ActionResult<List<BookingDto>>> GetByResource(int resourceId)
    {
        try
        {
            var bookings = await _bookingService.GetResourceBookingsAsync(resourceId);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for resource {ResourceId}", resourceId);
            return StatusCode(500, "An error occurred while retrieving bookings for the resource.");
        }
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<List<BookingDto>>> GetByEmployee(int employeeId)
    {
        try
        {
            var bookings = await _bookingService.GetEmployeeBookingsAsync(employeeId);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving bookings for the employee.");
        }
    }

    [HttpPost("check-conflict")]
    public async Task<ActionResult<BookingConflictResponse>> CheckConflict([FromBody] CreateBookingDto createDto)
    {
        try
        {
            var response = await _bookingService.CheckConflictAsync(
                createDto.ResourceId,
                createDto.StartTime,
                createDto.EndTime);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for conflicts");
            return StatusCode(500, "An error occurred while checking for booking conflicts.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingDto createDto)
    {
        try
        {
            var booking = await _bookingService.CreateBookingAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            return StatusCode(500, "An error occurred while creating the booking.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _bookingService.DeleteBookingAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking {Id}", id);
            return StatusCode(500, "An error occurred while deleting the booking.");
        }
    }
}

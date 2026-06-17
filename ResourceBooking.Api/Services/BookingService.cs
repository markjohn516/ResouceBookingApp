using ResourceBooking.Api.Repositories;
using ResourceBooking.Core.DTOs;
using ResourceBooking.Core.Models;

namespace ResourceBooking.Api.Services;

/// <summary>
/// Service interface for booking operations with business logic
/// </summary>
public interface IBookingService
{
    Task<BookingDto?> GetBookingAsync(int id);
    Task<List<BookingDto>> GetAllBookingsAsync();
    Task<List<BookingDto>> GetResourceBookingsAsync(int resourceId);
    Task<List<BookingDto>> GetEmployeeBookingsAsync(int employeeId);
    Task<BookingConflictResponse> CheckConflictAsync(int resourceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    Task<BookingDto> CreateBookingAsync(CreateBookingDto createDto);
    Task DeleteBookingAsync(int id);
}

/// <summary>
/// Booking service implementation with business logic
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IResourceRepository resourceRepository,
        IEmployeeRepository employeeRepository)
    {
        _bookingRepository = bookingRepository;
        _resourceRepository = resourceRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<BookingDto?> GetBookingAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        return booking == null ? null : MapToDto(booking);
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _bookingRepository.GetAllAsync();
        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingDto>> GetResourceBookingsAsync(int resourceId)
    {
        var bookings = await _bookingRepository.GetBookingsByResourceAsync(resourceId);
        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingDto>> GetEmployeeBookingsAsync(int employeeId)
    {
        var bookings = await _bookingRepository.GetBookingsByEmployeeAsync(employeeId);
        return bookings.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Checks if a booking would conflict with existing bookings
    /// </summary>
    public async Task<BookingConflictResponse> CheckConflictAsync(int resourceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        // Validate time range
        if (endTime <= startTime)
        {
            return new BookingConflictResponse
            {
                IsConflict = true,
                Message = "End time must be after start time."
            };
        }

        // Verify resource exists and is active
        var resource = await _resourceRepository.GetByIdAsync(resourceId);
        if (resource == null)
        {
            return new BookingConflictResponse
            {
                IsConflict = true,
                Message = "Resource not found."
            };
        }

        if (!resource.IsActive)
        {
            return new BookingConflictResponse
            {
                IsConflict = true,
                Message = "Resource is not active and cannot be booked."
            };
        }

        // Check for overlapping bookings
        var overlappingBookings = await _bookingRepository.GetOverlappingBookingsAsync(resourceId, startTime, endTime);
        
        // Filter out the booking we're updating if excludeBookingId is provided
        if (excludeBookingId.HasValue)
        {
            overlappingBookings = overlappingBookings.Where(b => b.Id != excludeBookingId.Value).ToList();
        }

        if (overlappingBookings.Any())
        {
            return new BookingConflictResponse
            {
                IsConflict = true,
                Message = $"Resource is already booked during this time period. {overlappingBookings.Count} conflicting booking(s) found.",
                ConflictingBookings = overlappingBookings.Select(MapToDto).ToList()
            };
        }

        return new BookingConflictResponse
        {
            IsConflict = false,
            Message = "No conflicts detected."
        };
    }

    /// <summary>
    /// Creates a new booking with full validation
    /// </summary>
    public async Task<BookingDto> CreateBookingAsync(CreateBookingDto createDto)
    {
        // Validate time range
        if (createDto.EndTime <= createDto.StartTime)
        {
            throw new InvalidOperationException("End time must be after start time.");
        }

        // Verify employee exists
        var employee = await _employeeRepository.GetByIdAsync(createDto.EmployeeId);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID {createDto.EmployeeId} not found.");
        }

        // Check for conflicts
        var conflictResponse = await CheckConflictAsync(createDto.ResourceId, createDto.StartTime, createDto.EndTime);
        if (conflictResponse.IsConflict)
        {
            throw new InvalidOperationException(conflictResponse.Message);
        }

        // Create booking
        var booking = new Booking
        {
            ResourceId = createDto.ResourceId,
            EmployeeId = createDto.EmployeeId,
            StartTime = createDto.StartTime,
            EndTime = createDto.EndTime,
            Note = createDto.Note,
            CreatedAt = DateTime.UtcNow
        };

        await _bookingRepository.AddAsync(booking);
        
        // Reload with navigation properties
        var createdBooking = await _bookingRepository.GetByIdAsync(booking.Id);
        return MapToDto(createdBooking!);
    }

    public async Task DeleteBookingAsync(int id)
    {
        await _bookingRepository.DeleteAsync(id);
    }

    private static BookingDto MapToDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            ResourceId = booking.ResourceId,
            EmployeeId = booking.EmployeeId,
            EmployeeName = booking.Employee?.FullName ?? "Unknown",
            ResourceName = booking.Resource?.Name ?? "Unknown",
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Note = booking.Note,
            CreatedAt = booking.CreatedAt
        };
    }
}

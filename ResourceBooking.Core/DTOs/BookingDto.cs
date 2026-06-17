namespace ResourceBooking.Core.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int EmployeeId { get; set; }
    public required string EmployeeName { get; set; }
    public required string ResourceName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBookingDto
{
    public int ResourceId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Note { get; set; }
}

public class BookingConflictResponse
{
    public bool IsConflict { get; set; }
    public string? Message { get; set; }
    public List<BookingDto>? ConflictingBookings { get; set; }
}

namespace ResourceBooking.Core.Models;

/// <summary>
/// Represents a booking of a resource by an employee
/// </summary>
public class Booking
{
    public int Id { get; set; }
    
    public int ResourceId { get; set; }
    public Resource? Resource { get; set; }
    
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime EndTime { get; set; }
    
    public string? Note { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

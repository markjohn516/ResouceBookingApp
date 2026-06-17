namespace ResourceBooking.Core.Models;

/// <summary>
/// Represents an employee in the system
/// </summary>
public class Employee
{
    public int Id { get; set; }
    
    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
    
    // Navigation property
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

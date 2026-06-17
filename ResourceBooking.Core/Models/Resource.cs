namespace ResourceBooking.Core.Models;

/// <summary>
/// Represents a shared resource that can be booked
/// </summary>
public class Resource
{
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    public required string Type { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

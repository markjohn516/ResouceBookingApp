namespace ResourceBooking.Core.DTOs;

public class ResourceDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateResourceDto
{
    public required string Name { get; set; }
    public required string Type { get; set; }
}

public class UpdateResourceDto
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public bool IsActive { get; set; }
}

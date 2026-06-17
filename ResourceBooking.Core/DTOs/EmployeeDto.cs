namespace ResourceBooking.Core.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}

public class CreateEmployeeDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}

public class UpdateEmployeeDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}

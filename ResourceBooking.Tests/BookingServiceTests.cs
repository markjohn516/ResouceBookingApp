using Moq;
using ResourceBooking.Api.Repositories;
using ResourceBooking.Api.Services;
using ResourceBooking.Core.DTOs;
using ResourceBooking.Core.Models;
using Xunit;

namespace ResourceBooking.Tests;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _mockBookingRepository;
    private readonly Mock<IResourceRepository> _mockResourceRepository;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _mockBookingRepository = new Mock<IBookingRepository>();
        _mockResourceRepository = new Mock<IResourceRepository>();
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();

        _bookingService = new BookingService(
            _mockBookingRepository.Object,
            _mockResourceRepository.Object,
            _mockEmployeeRepository.Object);
    }

    /// <summary>
    /// Test 1: Booking creation should succeed when no conflicts exist
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_WithNoConflicts_ShouldSucceed()
    {
        // Arrange
        var resourceId = 1;
        var employeeId = 1;
        var startTime = new DateTime(2024, 6, 17, 10, 0, 0);
        var endTime = new DateTime(2024, 6, 17, 11, 0, 0);

        var resource = new Resource { Id = resourceId, Name = "Meeting Room A", Type = "Room", IsActive = true };
        var employee = new Employee { Id = employeeId, FirstName = "John", LastName = "Doe" };
        
        var newBooking = new Booking
        {
            ResourceId = resourceId,
            EmployeeId = employeeId,
            StartTime = startTime,
            EndTime = endTime,
            Note = "Team meeting"
        };
        
        var createdBooking = new Booking
        {
            Id = 1,
            ResourceId = resourceId,
            EmployeeId = employeeId,
            StartTime = startTime,
            EndTime = endTime,
            Note = "Team meeting",
            Resource = resource,
            Employee = employee,
            CreatedAt = DateTime.UtcNow
        };

        _mockResourceRepository
            .Setup(x => x.GetByIdAsync(resourceId))
            .ReturnsAsync(resource);

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        _mockBookingRepository
            .Setup(x => x.GetOverlappingBookingsAsync(resourceId, startTime, endTime))
            .ReturnsAsync(new List<Booking>());

        _mockBookingRepository
            .Setup(x => x.AddAsync(It.IsAny<Booking>()))
            .ReturnsAsync(newBooking);

        _mockBookingRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(createdBooking);

        var createDto = new CreateBookingDto
        {
            ResourceId = resourceId,
            EmployeeId = employeeId,
            StartTime = startTime,
            EndTime = endTime,
            Note = "Team meeting"
        };

        // Act
        var result = await _bookingService.CreateBookingAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resourceId, result.ResourceId);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(startTime, result.StartTime);
        Assert.Equal(endTime, result.EndTime);
        Assert.Equal("Team meeting", result.Note);
        Assert.Equal("John Doe", result.EmployeeName);
    }

    /// <summary>
    /// Test 2: Booking creation should fail when time range is invalid
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_WithInvalidTimeRange_ShouldThrow()
    {
        // Arrange
        var createDto = new CreateBookingDto
        {
            ResourceId = 1,
            EmployeeId = 1,
            StartTime = new DateTime(2024, 6, 17, 11, 0, 0),
            EndTime = new DateTime(2024, 6, 17, 10, 0, 0), // End before start
            Note = null
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bookingService.CreateBookingAsync(createDto));

        Assert.Equal("End time must be after start time.", exception.Message);
    }

    /// <summary>
    /// Test 3: Booking conflict detection should identify overlapping bookings
    /// </summary>
    [Fact]
    public async Task CheckConflictAsync_WithOverlappingBookings_ShouldDetectConflict()
    {
        // Arrange
        var resourceId = 1;
        var requestedStart = new DateTime(2024, 6, 17, 10, 0, 0);
        var requestedEnd = new DateTime(2024, 6, 17, 11, 0, 0);

        var resource = new Resource { Id = resourceId, Name = "Meeting Room A", Type = "Room", IsActive = true };
        var existingBooking = new Booking
        {
            Id = 1,
            ResourceId = resourceId,
            StartTime = new DateTime(2024, 6, 17, 10, 30, 0),
            EndTime = new DateTime(2024, 6, 17, 11, 30, 0),
            Employee = new Employee { FirstName = "Jane", LastName = "Smith" }
        };

        _mockResourceRepository
            .Setup(x => x.GetByIdAsync(resourceId))
            .ReturnsAsync(resource);

        _mockBookingRepository
            .Setup(x => x.GetOverlappingBookingsAsync(resourceId, requestedStart, requestedEnd))
            .ReturnsAsync(new List<Booking> { existingBooking });

        // Act
        var result = await _bookingService.CheckConflictAsync(resourceId, requestedStart, requestedEnd);

        // Assert
        Assert.True(result.IsConflict);
        Assert.Contains("conflict", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(result.ConflictingBookings);
        Assert.Single(result.ConflictingBookings);
        Assert.Equal("Jane Smith", result.ConflictingBookings[0].EmployeeName);
    }

    /// <summary>
    /// Test 4: Booking creation should fail for disabled resources
    /// </summary>
    [Fact]
    public async Task CreateBookingAsync_WithDisabledResource_ShouldThrow()
    {
        // Arrange
        var resourceId = 1;
        var employeeId = 1;
        var resource = new Resource { Id = resourceId, Name = "Meeting Room A", Type = "Room", IsActive = false };
        var employee = new Employee { Id = employeeId, FirstName = "John", LastName = "Doe" };

        _mockResourceRepository
            .Setup(x => x.GetByIdAsync(resourceId))
            .ReturnsAsync(resource);

        _mockEmployeeRepository
            .Setup(x => x.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        var createDto = new CreateBookingDto
        {
            ResourceId = resourceId,
            EmployeeId = employeeId,
            StartTime = new DateTime(2024, 6, 17, 10, 0, 0),
            EndTime = new DateTime(2024, 6, 17, 11, 0, 0)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bookingService.CreateBookingAsync(createDto));

        Assert.Contains("not active", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test 5: Conflict detection should not detect conflicts when bookings are adjacent (no overlap)
    /// </summary>
    [Fact]
    public async Task CheckConflictAsync_WithAdjacentBookings_ShouldNotDetectConflict()
    {
        // Arrange
        var resourceId = 1;
        var requestedStart = new DateTime(2024, 6, 17, 11, 0, 0);
        var requestedEnd = new DateTime(2024, 6, 17, 12, 0, 0);

        var resource = new Resource { Id = resourceId, Name = "Meeting Room A", Type = "Room", IsActive = true };

        _mockResourceRepository
            .Setup(x => x.GetByIdAsync(resourceId))
            .ReturnsAsync(resource);

        _mockBookingRepository
            .Setup(x => x.GetOverlappingBookingsAsync(resourceId, requestedStart, requestedEnd))
            .ReturnsAsync(new List<Booking>()); // No overlapping bookings

        // Act
        var result = await _bookingService.CheckConflictAsync(resourceId, requestedStart, requestedEnd);

        // Assert
        Assert.False(result.IsConflict);
        Assert.Contains("no conflicts", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Resource service tests
/// </summary>
public class ResourceServiceTests
{
    private readonly Mock<IResourceRepository> _mockResourceRepository;
    private readonly ResourceService _resourceService;

    public ResourceServiceTests()
    {
        _mockResourceRepository = new Mock<IResourceRepository>();
        _resourceService = new ResourceService(_mockResourceRepository.Object);
    }

    [Fact]
    public async Task CreateResourceAsync_ShouldSucceed()
    {
        // Arrange
        var createDto = new CreateResourceDto
        {
            Name = "Conference Room B",
            Type = "Room"
        };

        var resource = new Resource
        {
            Id = 1,
            Name = createDto.Name,
            Type = createDto.Type,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockResourceRepository
            .Setup(x => x.AddAsync(It.IsAny<Resource>()))
            .ReturnsAsync(resource);

        // Act
        var result = await _resourceService.CreateResourceAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.Type, result.Type);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task DisableResourceAsync_ShouldSetIsActiveFalse()
    {
        // Arrange
        var resourceId = 1;
        var resource = new Resource { Id = resourceId, Name = "Meeting Room", Type = "Room", IsActive = true };

        _mockResourceRepository
            .Setup(x => x.GetByIdAsync(resourceId))
            .ReturnsAsync(resource);

        _mockResourceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Resource>()))
            .Returns(Task.CompletedTask);

        // Act
        await _resourceService.DisableResourceAsync(resourceId);

        // Assert
        Assert.False(resource.IsActive);
        _mockResourceRepository.Verify(x => x.UpdateAsync(It.IsAny<Resource>()), Times.Once);
    }
}

/// <summary>
/// Employee service tests
/// </summary>
public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _employeeService = new EmployeeService(_mockEmployeeRepository.Object);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldSucceed()
    {
        // Arrange
        var createDto = new CreateEmployeeDto
        {
            FirstName = "John",
            LastName = "Doe"
        };

        var employee = new Employee
        {
            Id = 1,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName
        };

        _mockEmployeeRepository
            .Setup(x => x.AddAsync(It.IsAny<Employee>()))
            .ReturnsAsync(employee);

        // Act
        var result = await _employeeService.CreateEmployeeAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.FirstName, result.FirstName);
        Assert.Equal(createDto.LastName, result.LastName);
        Assert.Equal("John Doe", result.FullName);
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("John", "")]
    public async Task CreateEmployeeAsync_WithMissingName_ShouldThrow(string firstName, string lastName)
    {
        // Arrange
        var createDto = new CreateEmployeeDto
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _employeeService.CreateEmployeeAsync(createDto));
    }
}

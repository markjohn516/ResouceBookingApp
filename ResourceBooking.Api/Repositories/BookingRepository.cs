using Microsoft.EntityFrameworkCore;
using ResourceBooking.Api.Data;
using ResourceBooking.Core.Models;

namespace ResourceBooking.Api.Repositories;

/// <summary>
/// Repository interface for Booking operations with specialized queries
/// </summary>
public interface IBookingRepository : IRepository<Booking>
{
    Task<List<Booking>> GetBookingsByResourceAsync(int resourceId);
    Task<List<Booking>> GetBookingsByEmployeeAsync(int employeeId);
    Task<List<Booking>> GetOverlappingBookingsAsync(int resourceId, DateTime startTime, DateTime endTime);
    Task<List<Booking>> GetActiveBookingsForResourceAsync(int resourceId);
}

/// <summary>
/// Booking repository implementation
/// </summary>
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ResourceBookingContext context) : base(context)
    {
    }

    public async Task<List<Booking>> GetBookingsByResourceAsync(int resourceId)
    {
        return await _dbSet
            .Where(b => b.ResourceId == resourceId)
            .Include(b => b.Employee)
            .Include(b => b.Resource)
            .OrderBy(b => b.StartTime)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetBookingsByEmployeeAsync(int employeeId)
    {
        return await _dbSet
            .Where(b => b.EmployeeId == employeeId)
            .Include(b => b.Employee)
            .Include(b => b.Resource)
            .OrderBy(b => b.StartTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets bookings that overlap with the given time period for a specific resource
    /// </summary>
    public async Task<List<Booking>> GetOverlappingBookingsAsync(int resourceId, DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(b => b.ResourceId == resourceId &&
                        b.StartTime < endTime &&
                        b.EndTime > startTime)
            .Include(b => b.Employee)
            .Include(b => b.Resource)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all active (not ended) bookings for a resource
    /// </summary>
    public async Task<List<Booking>> GetActiveBookingsForResourceAsync(int resourceId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(b => b.ResourceId == resourceId && b.EndTime > now)
            .Include(b => b.Employee)
            .Include(b => b.Resource)
            .OrderBy(b => b.StartTime)
            .ToListAsync();
    }
}

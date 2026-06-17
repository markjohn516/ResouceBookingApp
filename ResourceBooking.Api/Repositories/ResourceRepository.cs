using Microsoft.EntityFrameworkCore;
using ResourceBooking.Api.Data;
using ResourceBooking.Core.Models;

namespace ResourceBooking.Api.Repositories;

public interface IResourceRepository : IRepository<Resource>
{
    Task<List<Resource>> GetAllActiveAsync();
    Task<Resource?> GetWithBookingsAsync(int id);
}

public class ResourceRepository : Repository<Resource>, IResourceRepository
{
    public ResourceRepository(ResourceBookingContext context) : base(context)
    {
    }

    public async Task<List<Resource>> GetAllActiveAsync()
    {
        return await _dbSet
            .Where(r => r.IsActive)
            .OrderBy(r => r.Type)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Resource?> GetWithBookingsAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Bookings)
            .ThenInclude(b => b.Employee)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}

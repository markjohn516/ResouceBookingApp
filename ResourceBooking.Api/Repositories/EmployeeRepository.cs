using Microsoft.EntityFrameworkCore;
using ResourceBooking.Api.Data;
using ResourceBooking.Core.Models;

namespace ResourceBooking.Api.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<List<Employee>> GetAllActiveAsync();
}

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ResourceBookingContext context) : base(context)
    {
    }

    public async Task<List<Employee>> GetAllActiveAsync()
    {
        return await _dbSet
            .Include(e => e.Bookings)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }
}

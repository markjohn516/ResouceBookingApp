using Microsoft.EntityFrameworkCore;
using ResourceBooking.Core.Models;

namespace ResourceBooking.Api.Data;

public class ResourceBookingContext : DbContext
{
    public ResourceBookingContext(DbContextOptions<ResourceBookingContext> options) 
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Resource> Resources { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>()
            .HasKey(e => e.Id);

        // Resource configuration
        modelBuilder.Entity<Resource>()
            .HasKey(r => r.Id);

        // Booking configuration
        modelBuilder.Entity<Booking>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Resource)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Employee)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add seed data for testing
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed employees
        modelBuilder.Entity<Employee>().HasData(
            new Employee { Id = 1, FirstName = "John", LastName = "Doe" },
            new Employee { Id = 2, FirstName = "Jane", LastName = "Smith" },
            new Employee { Id = 3, FirstName = "Bob", LastName = "Johnson" }
        );

        // Seed resources with fixed datetime values
        var seedDateTime = new DateTime(2026, 6, 17, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Resource>().HasData(
            new Resource { Id = 1, Name = "Conference Room A", Type = "Room", IsActive = true, CreatedAt = seedDateTime },
            new Resource { Id = 2, Name = "Conference Room B", Type = "Room", IsActive = true, CreatedAt = seedDateTime },
            new Resource { Id = 3, Name = "Company Vehicle 1", Type = "Vehicle", IsActive = true, CreatedAt = seedDateTime },
            new Resource { Id = 4, Name = "Laptop 1", Type = "Equipment", IsActive = true, CreatedAt = seedDateTime }
        );
    }
}

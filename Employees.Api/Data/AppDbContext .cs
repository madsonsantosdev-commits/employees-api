using Employees.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Employees.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(150);
            e.Property(x => x.Document).IsRequired().HasMaxLength(20);
            e.Property(x => x.Email).IsRequired().HasMaxLength(120);

            e.HasIndex(x => x.Document).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });
    }
}

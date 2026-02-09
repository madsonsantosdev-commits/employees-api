using Employees.Api.Data;
using Employees.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Employees.Api.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;
    public EmployeeRepository(AppDbContext db) => _db = db;

    public Task<List<Employee>> GetAllAsync(CancellationToken ct) =>
        _db.Employees.AsNoTracking().OrderBy(x => x.FullName).ToListAsync(ct);

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Employees.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Employee?> GetByDocumentAsync(string document, CancellationToken ct) =>
        _db.Employees.FirstOrDefaultAsync(x => x.Document == document, ct);

    public Task<Employee?> GetByEmailAsync(string email, CancellationToken ct) =>
        _db.Employees.FirstOrDefaultAsync(x => x.Email == email, ct);

    public async Task AddAsync(Employee employee, CancellationToken ct)
    {
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken ct)
    {
        _db.Employees.Update(employee);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Employee employee, CancellationToken ct)
    {
        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync(ct);
    }
}

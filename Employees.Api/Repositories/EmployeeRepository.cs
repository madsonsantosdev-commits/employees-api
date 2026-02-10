using Employees.Api.Data;
using Employees.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Employees.Api.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;
    public EmployeeRepository(AppDbContext db) => _db = db;

    public async Task<List<Employee>> GetAllAsync(CancellationToken ct) =>
       await _db.Employees.AsNoTracking().OrderBy(x => x.FullName).ToListAsync(ct);

    public async Task<List<Employee>> GetAllAsync(string? search, int? page, int? pageSize, CancellationToken ct)
    {
        var query = _db.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.FullName, term) ||
                EF.Functions.Like(x.Document, term) ||
                EF.Functions.Like(x.Email, term));
        }

        query = query.OrderBy(x => x.FullName);

        if (page.HasValue || pageSize.HasValue)
        {
            var safePage = Math.Max(page ?? 1, 1);
            var safePageSize = Math.Clamp(pageSize ?? 50, 1, 200);
            query = query.Skip((safePage - 1) * safePageSize).Take(safePageSize);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<int> GetCountAsync(string? search, CancellationToken ct)
    {
        var query = _db.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.FullName, term) ||
                EF.Functions.Like(x.Document, term) ||
                EF.Functions.Like(x.Email, term));
        }

        return await query.CountAsync(ct);
    }

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

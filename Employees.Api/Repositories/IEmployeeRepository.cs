using Employees.Api.Models;

namespace Employees.Api.Repositories;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllAsync(CancellationToken ct);
    Task<List<Employee>> GetAllAsync(string? search, int? page, int? pageSize, CancellationToken ct);
    Task<int> GetCountAsync(string? search, CancellationToken ct);
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Employee?> GetByDocumentAsync(string document, CancellationToken ct);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct);
    Task AddAsync(Employee employee, CancellationToken ct);
    Task UpdateAsync(Employee employee, CancellationToken ct);
    Task DeleteAsync(Employee employee, CancellationToken ct);
}

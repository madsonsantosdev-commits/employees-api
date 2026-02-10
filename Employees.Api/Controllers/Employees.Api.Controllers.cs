using Employees.Api.Dtos;
using Employees.Api.Models;
using Employees.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Threading;

namespace Employees.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeRepository _repo;
    private readonly IMemoryCache _cache;
    private static CancellationTokenSource _cacheResetToken = new();
    private const string CacheKeyPrefix = "employees:list";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    public EmployeesController(IEmployeeRepository repo, IMemoryCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

    private static void ResetCache()
    {
        var old = Interlocked.Exchange(ref _cacheResetToken, new CancellationTokenSource());
        old.Cancel();
        old.Dispose();
    }

    private static string BuildCacheKey(string? search, int? page, int? pageSize) =>
        $"{CacheKeyPrefix}:search={search ?? string.Empty}:page={page?.ToString() ?? string.Empty}:pageSize={pageSize?.ToString() ?? string.Empty}";

    private sealed class EmployeeListCacheEntry
    {
        public List<Employee> Items { get; }
        public int Total { get; }

        public EmployeeListCacheEntry(List<Employee> items, int total)
        {
            Items = items;
            Total = total;
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<Employee>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var isPaged = page.HasValue || pageSize.HasValue;
        var cacheKey = BuildCacheKey(search, page, pageSize);

        if (_cache.TryGetValue(cacheKey, out EmployeeListCacheEntry? cached) && cached is not null)
        {
            if (isPaged)
                Response.Headers["X-Total-Count"] = cached.Total.ToString();

            return Ok(cached.Items);
        }

        var totalCount = isPaged ? await _repo.GetCountAsync(search, ct) : 0;
        var employees = await _repo.GetAllAsync(search, page, pageSize, ct);

        if (isPaged)
            Response.Headers["X-Total-Count"] = totalCount.ToString();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheTtl)
            .AddExpirationToken(new CancellationChangeToken(_cacheResetToken.Token));

        _cache.Set(cacheKey, new EmployeeListCacheEntry(employees, totalCount), cacheOptions);

        return Ok(employees);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Employee>> GetById(Guid id, CancellationToken ct)
    {
        var employee = await _repo.GetByIdAsync(id, ct);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> Create([FromBody] EmployeeCreateDto dto, CancellationToken ct)
    {
        if (await _repo.GetByDocumentAsync(dto.Document, ct) is not null)
            return Conflict(new { message = "Documento j� cadastrado." });

        if (await _repo.GetByEmailAsync(dto.Email, ct) is not null)
            return Conflict(new { message = "Email j� cadastrado." });

        var employee = new Employee
        {
            FullName = dto.FullName,
            Document = dto.Document,
            Email = dto.Email,
            HireDate = dto.HireDate,
            IsActive = true
        };

        try
        {
            await _repo.AddAsync(employee, ct);
            ResetCache();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Funcion�rio j� existe com o mesmo documento ou email." });
        }

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] EmployeeUpdateDto dto, CancellationToken ct)
    {
        var employee = await _repo.GetByIdAsync(id, ct);
        if (employee is null) return NotFound();

        var existingDocument = await _repo.GetByDocumentAsync(dto.Document, ct);
        if (existingDocument is not null && existingDocument.Id != id)
            return Conflict(new { message = "Documento j� cadastrado." });

        var existingEmail = await _repo.GetByEmailAsync(dto.Email, ct);
        if (existingEmail is not null && existingEmail.Id != id)
            return Conflict(new { message = "Email j� cadastrado." });

        employee.FullName = dto.FullName;
        employee.Document = dto.Document;
        employee.Email = dto.Email;
        employee.HireDate = dto.HireDate;
        employee.IsActive = dto.IsActive;

        try
        {
            await _repo.UpdateAsync(employee, ct);
            ResetCache();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Funcion�rio j� existe com o mesmo documento ou email." });
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var employee = await _repo.GetByIdAsync(id, ct);
        if (employee is null) return NotFound();

        await _repo.DeleteAsync(employee, ct);
        ResetCache();
        return NoContent();
    }
}

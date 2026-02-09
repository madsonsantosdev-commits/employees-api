using Employees.Api.Dtos;
using Employees.Api.Models;
using Employees.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Employees.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
	private readonly IEmployeeRepository _repo;
	public EmployeesController(IEmployeeRepository repo) => _repo = repo;

	[HttpGet]
	public async Task<ActionResult<List<Employee>>> GetAll(CancellationToken ct)
		=> Ok(await _repo.GetAllAsync(ct));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<Employee>> GetById(Guid id, CancellationToken ct)
	{
		var employee = await _repo.GetByIdAsync(id, ct);
		return employee is null ? NotFound() : Ok(employee);
	}

	[HttpPost]
	public async Task<ActionResult<Employee>> Create(EmployeeCreateDto dto, CancellationToken ct)
	{
		if (await _repo.GetByDocumentAsync(dto.Document, ct) is not null)
			return Conflict(new { message = "Document já cadastrado." });

		if (await _repo.GetByEmailAsync(dto.Email, ct) is not null)
			return Conflict(new { message = "Email já cadastrado." });

		var employee = new Employee
		{
			FullName = dto.FullName,
			Document = dto.Document,
			Email = dto.Email,
			HireDate = dto.HireDate,
			IsActive = true
		};

		await _repo.AddAsync(employee, ct);

		return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
	}

	[HttpPut("{id:guid}")]
	public async Task<ActionResult> Update(Guid id, EmployeeUpdateDto dto, CancellationToken ct)
	{
		var employee = await _repo.GetByIdAsync(id, ct);
		if (employee is null) return NotFound();

		// Regras simples: não validar duplicidade de email aqui sem comparar id
		var existingEmail = await _repo.GetByEmailAsync(dto.Email, ct);
		if (existingEmail is not null && existingEmail.Id != id)
			return Conflict(new { message = "Email já cadastrado." });

		employee.FullName = dto.FullName;
		employee.Email = dto.Email;
		employee.HireDate = dto.HireDate;
		employee.IsActive = dto.IsActive;

		await _repo.UpdateAsync(employee, ct);
		return NoContent();
	}

	[HttpDelete("{id:guid}")]
	public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
	{
		var employee = await _repo.GetByIdAsync(id, ct);
		if (employee is null) return NotFound();

		await _repo.DeleteAsync(employee, ct);
		return NoContent();
	}
}

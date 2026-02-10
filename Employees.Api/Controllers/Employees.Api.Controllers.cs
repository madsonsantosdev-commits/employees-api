using Employees.Api.Dtos;
using Employees.Api.Models;
using Employees.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
			return Conflict(new { message = "Document ja cadastrado." });

		if (await _repo.GetByEmailAsync(dto.Email, ct) is not null)
			return Conflict(new { message = "Email ja cadastrado." });

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
		}
		catch (DbUpdateException)
		{
			return Conflict(new { message = "Funcionario ja existe com o mesmo documento ou email." });
		}

		return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
	}

	[HttpPut("{id:guid}")]
	public async Task<ActionResult> Update(Guid id, EmployeeUpdateDto dto, CancellationToken ct)
	{
		var employee = await _repo.GetByIdAsync(id, ct);
		if (employee is null) return NotFound();

		var existingDocument = await _repo.GetByDocumentAsync(dto.Document, ct);
		if (existingDocument is not null && existingDocument.Id != id)
			return Conflict(new { message = "Document ja cadastrado." });

		var existingEmail = await _repo.GetByEmailAsync(dto.Email, ct);
		if (existingEmail is not null && existingEmail.Id != id)
			return Conflict(new { message = "Email ja cadastrado." });

		employee.FullName = dto.FullName;
		employee.Document = dto.Document;
		employee.Email = dto.Email;
		employee.HireDate = dto.HireDate;
		employee.IsActive = dto.IsActive;

		try
		{
			await _repo.UpdateAsync(employee, ct);
		}
		catch (DbUpdateException)
		{
			return Conflict(new { message = "Funcionario ja existe com o mesmo documento ou email." });
		}
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

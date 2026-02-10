namespace Employees.Api.Models;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = default!;
    public string Document { get; set; } = default!; // CPF ou outro
    public string Email { get; set; } = default!;
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
}

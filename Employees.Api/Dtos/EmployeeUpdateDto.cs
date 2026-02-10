namespace Employees.Api.Dtos;

public record EmployeeUpdateDto(
    string FullName,
    string Document,
    string Email,
    DateTime HireDate,
    bool IsActive
);

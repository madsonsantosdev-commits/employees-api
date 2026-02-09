namespace Employees.Api.Dtos;

public record EmployeeUpdateDto(
    string FullName,
    string Email,
    DateTime HireDate,
    bool IsActive
);

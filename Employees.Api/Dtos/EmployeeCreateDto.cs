namespace Employees.Api.Dtos;

public record EmployeeCreateDto(
    string FullName,
    string Document,
    string Email,
    DateTime HireDate
);

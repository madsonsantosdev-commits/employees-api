using Employees.Api.Controllers;
using Employees.Api.Dtos;
using Employees.Api.Models;
using Employees.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Employees.Api.Tests;

public class UnitTest1
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly EmployeesController _controller;
    private readonly IMemoryCache _cache;

    public UnitTest1()
    {
        _repositoryMock = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _controller = new EmployeesController(_repositoryMock.Object, _cache);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithEmployees()
    {
        // Arrange
        var ct = CancellationToken.None;
        var employees = new List<Employee>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FullName = "Ana Paula Ribeiro",
                Document = "12345678901",
                Email = "ana@empresa.com",
                HireDate = new DateTime(2023, 2, 15),
                IsActive = true
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, null, null, ct))
            .ReturnsAsync(employees);

        // Act
        var result = await _controller.GetAll(null, null, null, ct);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var ok = result.Result as OkObjectResult;
        ok!.Value.Should().BeAssignableTo<List<Employee>>();
        ((List<Employee>)ok.Value!).Should().HaveCount(1);

        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, ct))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _controller.GetById(id, ct);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();

        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenDocumentAlreadyExists()
    {
        // Arrange
        var ct = CancellationToken.None;
        var dto = new EmployeeCreateDto(
            "Novo Funcionario",
            "99999999999",
            "novo@empresa.com",
            new DateTime(2024, 1, 10)
        );

        _repositoryMock
            .Setup(r => r.GetByDocumentAsync(dto.Document, ct))
            .ReturnsAsync(new Employee { Id = Guid.NewGuid() });

        // Act
        var result = await _controller.Create(dto, ct);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();

        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        var ct = CancellationToken.None;
        var dto = new EmployeeCreateDto(
            "Diego Rocha",
            "88888888888",
            "diego@empresa.com",
            new DateTime(2022, 10, 30)
        );

        _repositoryMock.Setup(r => r.GetByDocumentAsync(dto.Document, ct))
                       .ReturnsAsync((Employee?)null);

        _repositoryMock.Setup(r => r.GetByEmailAsync(dto.Email, ct))
                       .ReturnsAsync((Employee?)null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Employee>(), ct))
                       .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(dto, ct);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        _repositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenEmployeeExists()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();
        var employee = new Employee { Id = id };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, ct))
            .ReturnsAsync(employee);

        _repositoryMock
            .Setup(r => r.DeleteAsync(employee, ct))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(id, ct);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _repositoryMock.VerifyAll();
    }
}

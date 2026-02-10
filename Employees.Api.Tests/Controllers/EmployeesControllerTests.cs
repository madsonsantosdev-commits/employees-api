using Employees.Api.Controllers;
using Employees.Api.Dtos;
using Employees.Api.Models;
using Employees.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Employees.Api.Tests.Controllers;

public class EmployeesControllerTests
{
    private readonly Mock<IEmployeeRepository> _repoMock;
    private readonly EmployeesController _controller;
    private readonly IMemoryCache _cache;

    public EmployeesControllerTests()
    {
        _repoMock = new Mock<IEmployeeRepository>(MockBehavior.Strict);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _controller = new EmployeesController(_repoMock.Object, _cache);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenValid()
    {
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();

        var existing = new Employee
        {
            Id = id,
            FullName = "Nome Antigo",
            Document = "12345678999",
            Email = "antigo@empresa.com",
            HireDate = new DateTime(2020, 1, 1),
            IsActive = true
        };

        var dto = new EmployeeUpdateDto(
            FullName: "Nome Novo",
            Document: "12345678999",
            Email: "novo@empresa.com",
            HireDate: new DateTime(2021, 1, 1),
            IsActive: false);

        _repoMock.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.GetByDocumentAsync(dto.Document, ct)).ReturnsAsync((Employee?)null);
        _repoMock.Setup(r => r.GetByEmailAsync(dto.Email, ct)).ReturnsAsync((Employee?)null);
        _repoMock.Setup(r => r.UpdateAsync(existing, ct)).Returns(Task.CompletedTask);

        var result = await _controller.Update(id, dto, ct);

        result.Should().BeOfType<NoContentResult>();
        existing.FullName.Should().Be(dto.FullName);
        existing.Document.Should().Be(dto.Document);
        existing.Email.Should().Be(dto.Email);
        existing.HireDate.Should().Be(dto.HireDate);
        existing.IsActive.Should().Be(dto.IsActive);

        _repoMock.VerifyAll();
    }

    [Fact]
    public async Task Update_ReturnsConflict_WhenDocumentExists()
    {
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        var existing = new Employee { Id = id };
        var dto = new EmployeeUpdateDto(
            FullName: "Nome",
            Document: "11111111111",
            Email: "novo@empresa.com",
            HireDate: new DateTime(2021, 1, 1),
            IsActive: true);

        _repoMock.Setup(r => r.GetByIdAsync(id, ct)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.GetByDocumentAsync(dto.Document, ct))
            .ReturnsAsync(new Employee { Id = otherId });

        var result = await _controller.Update(id, dto, ct);

        result.Should().BeOfType<ConflictObjectResult>();

        _repoMock.VerifyAll();
    }
}

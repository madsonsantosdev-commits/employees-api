# employees-api
Autor: Madson Santos - 2026-02-09 00:00
API REST em .NET 8 (ASP.NET Core) para cadastro de funcionarios, com EF Core (SQLite), validacao basica e Swagger.

## Visao geral
- CRUD completo de funcionarios (criar, listar, obter por id, atualizar, remover)
- Persistencia com SQLite via EF Core
- Documentacao interativa com Swagger

## Stack
- .NET 8 / ASP.NET Core
- EF Core + SQL Server
- Swashbuckle.AspNetCore (Swagger)

## Como executar
1. Restaurar dependencias:
	- `dotnet restore Employees.Api/Employees.Api.csproj`
2. Compilar:
	- `dotnet build Employees.Api/Employees.Api.csproj`
3. Rodar:
	- `dotnet run --project Employees.Api/Employees.Api.csproj`

## Swagger
Com a aplicacao em execucao, acesse:
- http://localhost:5196/swagger

## Endpoints principais
- GET /api/employees
- GET /api/employees/{id}
- POST /api/employees
- PUT /api/employees/{id}
- DELETE /api/employees/{id}

## Exemplos de payload
Criar funcionario (POST /api/employees):
```json
{
	"fullName": "Maria Silva",
	"document": "12345678901",
	"email": "maria@empresa.com",
	"hireDate": "2024-02-01T00:00:00"
}
```

Atualizar funcionario (PUT /api/employees/{id}):
```json
{
	"fullName": "Maria Silva",
	"document": "12345678901",
	"email": "maria.nova@empresa.com",
	"hireDate": "2024-02-01T00:00:00",
	"isActive": true
}
```

## Regras de validacao
- `document` e `email` devem ser unicos
- `hireDate` e obrigatorio

## Testes
Para rodar os testes:
- `dotnet test Employees.Api.Tests/Employees.Api.Tests.csproj`

using Employees.Api.Data;
using Employees.Api.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers (MVC)
builder.Services.AddControllers();

// Swagger (OpenAPI via Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection string (com validação)
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' não encontrada. Configure no appsettings.json.");
}

// EF Core (SQLite)
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connString));

// DI
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

var app = builder.Build();

// Swagger somente em Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

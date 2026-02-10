using Employees.Api.Data;
using Employees.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Controllers (MVC)
builder.Services.AddControllers();

// Swagger (OpenAPI via Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employees API",
        Version = "v1",
        Description = "CRUD de funcionários - Madson Santos"
    });

    // XML Comments (opcional)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Connection string (com validação)
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' não encontrada. Configure no appsettings.json.");
}

// EF Core (SQL Server 2025)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(connString, sql => sql.EnableRetryOnFailure())
);

// DI
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

var app = builder.Build();

// Swagger somente em Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employees API v1");
        c.DocumentTitle = "Employees API";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

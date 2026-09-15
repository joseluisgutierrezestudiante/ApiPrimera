using ApiPrimera.DB;
using ApiPrimera.Repository;
using ApiPrimera.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configurar DbContext y repositorio para Producto (EF Core + MySQL)
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connection))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connection, ServerVersion.AutoDetect(connection)));
    builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

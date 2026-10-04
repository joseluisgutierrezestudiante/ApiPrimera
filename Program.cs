using ApiPrimera.DB;
using ApiPrimera.Repository;
using ApiPrimera.Interfaces;
using ApiPrimera.Services;
using ApiPrimera.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configurar DbContext y repositorio para Producto (EF Core + MySQL)
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connection))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connection, ServerVersion.AutoDetect(connection)));
    builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
}

// Configurar Cloudinary (subida de imágenes)
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();

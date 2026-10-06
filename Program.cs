using ApiPrimera.DB;
using ApiPrimera.Interfaces;
using ApiPrimera.Repository;
using ApiPrimera.Services;
using ApiPrimera.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

const string CorsPolicy = "AllowFrontend";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// CORS: permitir que el frontend consuma /api/producto
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Configurar DbContext y repositorio para Producto (EF Core + MySQL)
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connection))
{
    var mensaje =
        "Falta la cadena de conexión 'DefaultConnection'. Sin ella la app no puede arrancar.\n" +
        "\n" +
        "Opción recomendada (no versiona la clave):\n" +
        "  dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" " +
        "\"Server=localhost;Port=3306;Database=EcommerceDB;User=root;Password=TU_CLAVE;\"\n" +
        "\n" +
        "O define la variable de entorno ConnectionStrings__DefaultConnection.\n" +
        "Consulta el README para el detalle.";

    throw new InvalidOperationException(mensaje);
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 46))));
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

// Autenticación con ASP.NET Core Identity (cookies de sesión sobre MySQL).
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
    {
        opciones.Password.RequiredLength = 8;
        opciones.Password.RequireDigit = true;
        opciones.Password.RequireUppercase = true;
        opciones.Password.RequireLowercase = true;
        opciones.Password.RequireNonAlphanumeric = false;

        opciones.User.RequireUniqueEmail = true;

        opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        opciones.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Login";
    opciones.LogoutPath = "/Logout";
    opciones.AccessDeniedPath = "/Login";
    opciones.ExpireTimeSpan = TimeSpan.FromDays(7);
    opciones.SlidingExpiration = true;
});

// Configurar Cloudinary (subida de imágenes)
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// Necesario para que ProductoApiClient resuelva el host de la petición en curso
// cuando no se configura ApiAutoconsumida:BaseUrl.
builder.Services.AddHttpContextAccessor();

// Carrito de compras persistido en la sesión del navegador.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
    opciones.IdleTimeout = TimeSpan.FromDays(7);
});
builder.Services.AddScoped<ICarritoService, CarritoService>();

// Las vistas (Home, Producto, Admin) consumen la API de productos por HTTP.
// ApiAutoconsumida:BaseUrl es opcional: si no se define, ProductoApiClient usa el
// host de la petición en curso, de modo que funciona en cualquier puerto.
builder.Services.AddHttpClient<IProductoApiClient, ProductoApiClient>();

// Descarga de imágenes para el seed del catálogo
builder.Services.AddHttpClient("catalogo-origenes", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddScoped<IProductoSeedService, ProductoSeedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

// Sembrar el catálogo la primera vez que la base de datos está vacía.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
    try
    {
        var seed = scope.ServiceProvider.GetRequiredService<IProductoSeedService>();
        await seed.SemearAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudo sembrar el catálogo: {Mensaje}", ex.Message);
    }
}

app.Run();
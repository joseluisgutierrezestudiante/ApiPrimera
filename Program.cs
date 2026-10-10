using ApiPrimera.DB;
using ApiPrimera.Interfaces;
using ApiPrimera.Repository;
using ApiPrimera.Services;
using ApiPrimera.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

const string CorsPolicy = "AllowFrontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddOpenApi();

// CORS: el frontend (estático o Razor) consume /api. En Desarrollo se refleja
// cualquier origen para permitir el envío de la cookie de sesión; en Producción
// usa la lista Cors:AllowedOrigins de la configuración (obligatoria para cookies).
var origenesPermitidos = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true);
        }
        else if (origenesPermitidos.Length > 0)
        {
            policy.WithOrigins(origenesPermitidos);
        }
        else
        {
            policy.SetIsOriginAllowed(_ => false);
        }

        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Límite de peticiones para los endpoints sensibles de autenticación
// (mitiga fuerza bruta y creación masiva de cuentas).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });
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
    options.UseMySql(connection, ServerVersion.AutoDetect(connection)));
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
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.SameSite = SameSiteMode.Lax;
    opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Autenticación JWT para la API (además de la cookie de Identity).
// El secreto vive SOLO en variables de entorno (Jwt__SecretKey) o en
// user-secrets; nunca en appsettings.json, que se versiona en el repositorio.
var jwt = builder.Configuration.GetSection(JwtSettings.Seccion).Get<JwtSettings>() ?? new JwtSettings();
if (!jwt.EstaConfigurada)
{
    throw new InvalidOperationException(
        "Falta la clave 'Jwt:SecretKey' (mínimo 32 caracteres) para firmar los tokens JWT.\n" +
        "\n" +
        "Opción recomendada (no versiona la clave):\n" +
        "  dotnet user-secrets set \"Jwt:SecretKey\" \"<clave aleatoria de 32+ caracteres>\"\n" +
        "\n" +
        "O define la variable de entorno Jwt__SecretKey:\n" +
        "  PowerShell:  $env:Jwt__SecretKey=\"<clave>\"\n" +
        "  Linux/macOS: export Jwt__SecretKey=\"<clave>\"\n" +
        "\n" +
        "Consulta el README para el detalle.");
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.Seccion));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Access tokens de vida corta (15 min, HS256). El esquema "Bearer" se agrega
// sin tocar los defaults de Identity: la cookie de sesión sigue igual.
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            // Tolerancia mínima: con tokens de 15 minutos no hace falta más.
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

// Política de API: acepta la cookie de Identity O el Bearer JWT.
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("ApiConJwt", politica =>
    {
        politica.RequireAuthenticatedUser();
        politica.AddAuthenticationSchemes(
            IdentityConstants.ApplicationScheme,
            JwtBearerDefaults.AuthenticationScheme);
    });
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

// Cabeceras de seguridad básicas (anti clickjacking, sniffing y fugas de referer).
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    await next();
});

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

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
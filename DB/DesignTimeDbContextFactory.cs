using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiPrimera.DB;

// Fábrica de diseño para permitir crear migraciones sin depender de la configuración en tiempo de ejecución.
// Usa una cadena de conexión de ejemplo que debe ajustarse localmente antes de aplicar la migración a la base.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Cadena de conexión de ejemplo (NO subir credenciales reales). Ajusta antes de usar dotnet ef database update.
        var connectionString = "Server=localhost;Database=EcommerceDB;User=root;Password=changeme;";
        // Evitar AutoDetect para no intentar conectarse en tiempo de diseño con credenciales de ejemplo
        var serverVersion = ServerVersion.Parse("8.0.33-mysql");
        optionsBuilder.UseMySql(connectionString, serverVersion);
        return new AppDbContext(optionsBuilder.Options);
    }
}

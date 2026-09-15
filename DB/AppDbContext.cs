using Microsoft.EntityFrameworkCore;
using ApiPrimera.Models;

namespace ApiPrimera.DB;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Producto> Producto { get; set; } = null!;
}

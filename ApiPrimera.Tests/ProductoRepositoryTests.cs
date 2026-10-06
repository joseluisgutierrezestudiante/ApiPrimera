using ApiPrimera.Repository;
using Xunit;

namespace ApiPrimera.Tests;

public class ProductoRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ordena_destacados_primero()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        ctx.CrearProducto(nombre: "B normal", destacado: false, precio: 10m);
        ctx.CrearProducto(nombre: "A destacado", destacado: true, precio: 20m);
        ctx.CrearProducto(nombre: "C normal", destacado: false, precio: 5m);

        var lista = (await repo.GetAllAsync()).ToList();

        Assert.Equal("A destacado", lista[0].Nombre);
        Assert.Equal(3, lista.Count);
    }

    [Theory]
    [InlineData("Corolla")]
    [InlineData("Toyota")]
    [InlineData("Sedan compacto")]
    [InlineData("  Corolla  ")]
    public async Task BuscarAsync_encuentra_por_cualquier_campo(string busqueda)
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        ctx.CrearProducto(nombre: "Toyota Corolla", marca: "Toyota", categoria: "Sedan", descripcion: "Sedan compacto");

        var lista = await repo.BuscarAsync(busqueda, null, null, null);

        Assert.Single(lista);
    }

    

    [Fact]
    public async Task BuscarAsync_devuelve_vacio_si_nada_coincide()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        ctx.CrearProducto(nombre: "Toyota Corolla");

        var lista = await repo.BuscarAsync("no existe", null, null, null);

        Assert.Empty(lista);
    }

    [Fact]
    public async Task BuscarAsync_filtra_por_marca_y_categoria()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        ctx.CrearProducto(nombre: "Corolla", marca: "Toyota", categoria: "Sedan");
        ctx.CrearProducto(nombre: "Hilux", marca: "Toyota", categoria: "Camioneta");
        ctx.CrearProducto(nombre: "Civic", marca: "Honda", categoria: "Sedan");

        Assert.Equal(2, (await repo.BuscarAsync(null, "Toyota", null, null)).Count());
        Assert.Equal(2, (await repo.BuscarAsync(null, null, "Sedan", null)).Count());
        Assert.Equal(1, (await repo.BuscarAsync(null, "Toyota", "Camioneta", null)).Count());
    }

    [Theory]
    [InlineData("nombre-asc", "A")]
    [InlineData("   ", "A")] // sin orden: destacados primero, luego por nombre
    [InlineData("orden-desconocido", "A")]
    public async Task BuscarAsync_respeta_el_orden_por_nombre(string orden, string primerNombre)
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        ctx.CrearProducto(nombre: "B", precio: 50m);
        ctx.CrearProducto(nombre: "C", precio: 99m);
        ctx.CrearProducto(nombre: "A", precio: 10m);

        var lista = (await repo.BuscarAsync(null, null, null, orden)).ToList();

        Assert.Equal(primerNombre, lista[0].Nombre);
    }

    [Theory]
    [InlineData("anio-desc", 2025)]
    public async Task BuscarAsync_ordena_por_anio(string orden, int primerAnio)
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        ctx.CrearProducto(nombre: "Viejo", anio: 2015);
        ctx.CrearProducto(nombre: "Nuevo", anio: 2025);
        ctx.CrearProducto(nombre: "Medio", anio: 2020);

        var lista = (await repo.BuscarAsync(null, null, null, orden)).ToList();

        Assert.Equal(primerAnio, lista[0].Anio);
    }

    [Fact]
    public async Task BuscarAsync_sin_orden_pone_los_destacados_primero()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        ctx.CrearProducto(nombre: "Normal", destacado: false);
        ctx.CrearProducto(nombre: "Estrella", destacado: true);
        ctx.CrearProducto(nombre: "Otro", destacado: false);

        var lista = (await repo.BuscarAsync(null, null, null, null)).ToList();

        Assert.Equal("Estrella", lista[0].Nombre);
    }

    // Nota: el orden por precio (precio-asc / precio-desc) no se prueba aqui porque
    // SQLite rechaza ORDER BY sobre decimal. MySQL, que es el motor de produccion,
    // si lo soporta. Si se migra el repositorio a PostgreSQL u otro motor, este
    // test se puede reincorporar sin cambios.

    [Fact]
    public async Task BuscarAsync_ignora_filtros_vacios_o_con_espacios()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        ctx.CrearProducto(nombre: "Corolla");

        var lista = await repo.BuscarAsync("   ", "  ", "", "   ");

        Assert.Single(lista);
    }

    [Fact]
    public async Task GetMarcasAsync_y_GetCategoriasAsync_devuelven_valores_unicos_ordenados()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        ctx.CrearProducto(marca: "Toyota", categoria: "Sedan");
        ctx.CrearProducto(marca: "Toyota", categoria: "Camioneta");
        ctx.CrearProducto(marca: "Honda", categoria: "Sedan");

        var marcas = (await repo.GetMarcasAsync()).ToList();
        var categorias = (await repo.GetCategoriasAsync()).ToList();

        Assert.Equal(new[] { "Honda", "Toyota" }, marcas);
        Assert.Equal(new[] { "Camioneta", "Sedan" }, categorias);
    }

    [Fact]
    public async Task GetByIdAsync_devuelve_null_si_no_existe()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        Assert.Null(await repo.GetByIdAsync(999));
    }

    [Fact]
    public async Task CreateAsync_asigna_el_id_de_la_base_de_datos()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        var creado = await repo.CreateAsync(new ApiPrimera.Models.Producto
        {
            Nombre = "Nuevo",
            Marca = "Kia",
            Precio = 42_000_000m,
            Stock = 2
        });

        Assert.True(creado.Id > 0);
        Assert.Equal("Nuevo", (await repo.GetByIdAsync(creado.Id))!.Nombre);
    }

    [Fact]
    public async Task UpdateAsync_actualiza_los_campos_editables()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        var original = ctx.CrearProducto(nombre: "Nombre viejo", precio: 10m, stock: 1);

        var cambios = new ApiPrimera.Models.Producto
        {
            Id = original.Id,
            Nombre = "Nombre nuevo",
            Marca = "Kia",
            Categoria = "SUV",
            Anio = 2025,
            Kilometraje = 900,
            Combustible = "Híbrido",
            Transmision = "Automática",
            Color = "Negro",
            Precio = 77_000_000m,
            PrecioOriginal = 80_000_000m,
            Stock = 9,
            Destacado = true
        };

        Assert.True(await repo.UpdateAsync(cambios));

        var guardado = await repo.GetByIdAsync(original.Id);

        Assert.Equal("Nombre nuevo", guardado!.Nombre);
        Assert.Equal("Kia", guardado.Marca);
        Assert.Equal(2025, guardado.Anio);
        Assert.Equal(9, guardado.Stock);
        Assert.True(guardado.Destacado);
    }

    [Fact]
    public async Task UpdateAsync_devuelve_false_si_el_producto_no_existe()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        var ok = await repo.UpdateAsync(new ApiPrimera.Models.Producto { Id = 4242, Nombre = "X", Marca = "Y" });

        Assert.False(ok);
    }

    [Fact]
    public async Task UpdateAsync_no_puede_cambiar_la_imagen_y_deja_el_gestor_de_imagen_como_unico_dueño()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        var original = ctx.CrearProducto(nombre: "Con imagen");
        Assert.True(await repo.SetImagenUrlAsync(original.Id, "https://res.cloudinary.com/demo/image/upload/productos/foto.jpg"));

        var intruso = new ApiPrimera.Models.Producto
        {
            Id = original.Id,
            Nombre = "Con imagen",
            Marca = original.Marca,
            Precio = original.Precio,
            ImagenUrl = "https://ejemplo.com/no-deberia-guardarse.jpg"
        };

        Assert.True(await repo.UpdateAsync(intruso));

        var guardado = await repo.GetByIdAsync(original.Id);

        Assert.Equal(
            "https://res.cloudinary.com/demo/image/upload/productos/foto.jpg",
            guardado!.ImagenUrl);
    }

    [Fact]
    public async Task SetImagenUrlAsync_acepta_null_para_quitar_la_imagen()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        var producto = ctx.CrearProducto();
        Assert.True(await repo.SetImagenUrlAsync(producto.Id, "https://res.cloudinary.com/demo/image/upload/productos/a.jpg"));
        Assert.True(await repo.SetImagenUrlAsync(producto.Id, null));

        Assert.Null((await repo.GetByIdAsync(producto.Id))!.ImagenUrl);
    }

    [Fact]
    public async Task SetImagenUrlAsync_devuelve_false_si_no_existe()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        Assert.False(await repo.SetImagenUrlAsync(777, "https://x/y.jpg"));
    }

    [Fact]
    public async Task DeleteAsync_elimina_y_luego_devuelve_false()
    {
        using var ctx = new ContextoPrueba();
        var repo = new ProductoRepository(ctx.Contexto);

        var producto = ctx.CrearProducto();

        Assert.True(await repo.DeleteAsync(producto.Id));
        Assert.Null(await repo.GetByIdAsync(producto.Id));
        Assert.False(await repo.DeleteAsync(producto.Id));
    }
}
using Microsoft.AspNetCore.Mvc;
using ApiPrimera.Interfaces;
using ApiPrimera.Models;

namespace ApiPrimera.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductoController : ControllerBase
{
    private readonly IProductoRepository _repo;

    public ProductoController(IProductoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetAll()
    {
        var lista = await _repo.GetAllAsync();
        return Ok(lista);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> Get(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p == null) return NotFound();
        return Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Create([FromBody] Producto producto)
    {
        // validaciones básicas
        if (string.IsNullOrWhiteSpace(producto.Nombre)) return BadRequest(new { error = "El nombre es obligatorio." });
        if (producto.Precio < 0) return BadRequest(new { error = "El precio no puede ser negativo." });
        if (producto.Stock < 0) return BadRequest(new { error = "El stock no puede ser negativo." });

        var created = await _repo.CreateAsync(producto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Producto producto)
    {
        if (id != producto.Id) return BadRequest();
        if (string.IsNullOrWhiteSpace(producto.Nombre)) return BadRequest(new { error = "El nombre es obligatorio." });
        if (producto.Precio < 0) return BadRequest(new { error = "El precio no puede ser negativo." });
        if (producto.Stock < 0) return BadRequest(new { error = "El stock no puede ser negativo." });

        var ok = await _repo.UpdateAsync(producto);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}

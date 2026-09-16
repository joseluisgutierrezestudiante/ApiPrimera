using Microsoft.AspNetCore.Mvc;
using ApiPrimera.Models;
using ApiPrimera.Data;

namespace ApiPrimera.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarroController : ControllerBase
{
    // Controlador simple que actúa directamente sobre InMemoryData

    [HttpGet]
    public ActionResult<IEnumerable<Carro>> GetAll() => Ok(InMemoryData.Carros);

    [HttpGet("{id}")]
    public ActionResult<Carro> Get(int id)
    {
        var c = InMemoryData.Carros.FirstOrDefault(x => x.Id == id);
        if (c == null) return NotFound();
        // incluir información de la marca en la respuesta
        c.Marca = InMemoryData.Marcas.FirstOrDefault(m => m.Id == c.MarcaId);
        return Ok(c);
    }

    [HttpPost]
    public ActionResult<Carro> Create([FromBody] Carro carro)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var marca = InMemoryData.Marcas.FirstOrDefault(m => m.Id == carro.MarcaId);
        if (marca == null) return BadRequest("La marca indicada no existe.");

        carro.Id = InMemoryData.Carros.Any() ? InMemoryData.Carros.Max(c => c.Id) + 1 : 1;
        carro.Marca = marca;
        // aplicar descuento automáticamente según la marca
        var pct = marca.DiscountPercentage;
        carro.PrecioConDescuento = Math.Round(carro.Precio * (1 - pct / 100m), 2);

        InMemoryData.Carros.Add(carro);
        return CreatedAtAction(nameof(Get), new { id = carro.Id }, carro);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Carro carro)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var existing = InMemoryData.Carros.FirstOrDefault(x => x.Id == id);
        if (existing == null) return NotFound();

        var marca = InMemoryData.Marcas.FirstOrDefault(m => m.Id == carro.MarcaId);
        if (marca == null) return BadRequest("La marca indicada no existe.");

        existing.Modelo = carro.Modelo;
        existing.Color = carro.Color;
        existing.Placa = carro.Placa;
        existing.Precio = carro.Precio;
        existing.MarcaId = carro.MarcaId;
        existing.Marca = marca;
        var pct = marca.DiscountPercentage;
        existing.PrecioConDescuento = Math.Round(existing.Precio * (1 - pct / 100m), 2);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existing = InMemoryData.Carros.FirstOrDefault(x => x.Id == id);
        if (existing == null) return NotFound();
        InMemoryData.Carros.Remove(existing);
        return NoContent();
    }
}

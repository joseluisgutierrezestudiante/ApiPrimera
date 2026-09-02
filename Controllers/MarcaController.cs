using Microsoft.AspNetCore.Mvc;
using ApiPrimera.Models;
using ApiPrimera.Data;

namespace ApiPrimera.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcaController : ControllerBase
{
    // Versión básica: validaciones mínimas y respuestas sencillas

    [HttpGet]
    public ActionResult<IEnumerable<Marca>> GetAll() => Ok(InMemoryData.Marcas);

    [HttpGet("{id}")]
    public ActionResult<Marca> Get(int id)
    {
        var m = InMemoryData.Marcas.FirstOrDefault(x => x.Id == id);
        if (m == null) return NotFound();
        return Ok(m);
    }

    [HttpPost]
    public ActionResult<Marca> Create([FromBody] Marca marca)
    {
        // validaciones simples
        if (string.IsNullOrWhiteSpace(marca.Nombre) || string.IsNullOrWhiteSpace(marca.Descripcion))
            return BadRequest(new { error = "Nombre y descripcion son obligatorios." });

        marca.Id = InMemoryData.Marcas.Any() ? InMemoryData.Marcas.Max(m => m.Id) + 1 : 1;
        marca.DiscountPercentage = marca.Nombre?.Trim().ToLower() switch
        {
            "toyota" => 15m,
            "renault" => 25m,
            "chevrolet" => 20m,
            _ => 0m
        };
        InMemoryData.Marcas.Add(marca);
        return Ok(marca);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Marca marca)
    {
        var existing = InMemoryData.Marcas.FirstOrDefault(x => x.Id == id);
        if (existing == null) return NotFound();
        if (string.IsNullOrWhiteSpace(marca.Nombre) || string.IsNullOrWhiteSpace(marca.Descripcion))
            return BadRequest(new { error = "Nombre y descripcion son obligatorios." });

        existing.Nombre = marca.Nombre;
        existing.Descripcion = marca.Descripcion;
        existing.DiscountPercentage = marca.Nombre?.Trim().ToLower() switch
        {
            "toyota" => 15m,
            "renault" => 25m,
            "chevrolet" => 20m,
            _ => 0m
        };
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existing = InMemoryData.Marcas.FirstOrDefault(x => x.Id == id);
        if (existing == null) return NotFound();
        if (InMemoryData.Carros.Any(c => c.MarcaId == id)) return BadRequest(new { error = "No se puede eliminar una marca que tiene carros asociados." });
        InMemoryData.Marcas.Remove(existing);
        return Ok();
    }
}

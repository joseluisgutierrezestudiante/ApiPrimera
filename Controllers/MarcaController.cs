using Microsoft.AspNetCore.Mvc;
using ApiPrimera.Interfaces;
using ApiPrimera.Models;

namespace ApiPrimera.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcaController : ControllerBase
{
    private readonly IMarcaRepository _repo;

    public MarcaController(IMarcaRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Marca>> GetAll() => Ok(_repo.GetAll());

    [HttpGet("{id}")]
    public ActionResult<Marca> Get(int id)
    {
        var m = _repo.GetById(id);
        if (m == null) return NotFound();
        return Ok(m);
    }

    [HttpPost]
    public ActionResult<Marca> Create(Marca marca)
    {
        var created = _repo.Create(marca);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Marca marca)
    {
        if (id != marca.Id) return BadRequest();
        var ok = _repo.Update(marca);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var ok = _repo.Delete(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}

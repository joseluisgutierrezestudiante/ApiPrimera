using Microsoft.AspNetCore.Mvc;
using ApiPrimera.Interfaces;
using ApiPrimera.Models;

namespace ApiPrimera.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarroController : ControllerBase
{
    private readonly ICarroRepository _repo;

    public CarroController(ICarroRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Carro>> GetAll() => Ok(_repo.GetAll());

    [HttpGet("{id}")]
    public ActionResult<Carro> Get(int id)
    {
        var c = _repo.GetById(id);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public ActionResult<Carro> Create(Carro carro)
    {
        try
        {
            var created = _repo.Create(carro);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Carro carro)
    {
        if (id != carro.Id) return BadRequest();
        try
        {
            var ok = _repo.Update(carro);
            if (!ok) return NotFound();
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var ok = _repo.Delete(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}

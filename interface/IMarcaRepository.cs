using ApiPrimera.Models;

namespace ApiPrimera.Interfaces;

public interface IMarcaRepository
{
    IEnumerable<Marca> GetAll();
    Marca? GetById(int id);
    Marca Create(Marca marca);
    bool Update(Marca marca);
    bool Delete(int id);
}

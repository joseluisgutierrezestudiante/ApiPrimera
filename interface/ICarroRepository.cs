using ApiPrimera.Models;

namespace ApiPrimera.Interfaces;

public interface ICarroRepository
{
    IEnumerable<Carro> GetAll();
    Carro? GetById(int id);
    Carro Create(Carro carro);
    bool Update(Carro carro);
    bool Delete(int id);
}

using ApiPrimera.Data;
using ApiPrimera.Interfaces;

namespace ApiPrimera.Services;

public interface IProductoSeedService
{
    Task<int> SemearAsync(CancellationToken cancellationToken = default);
}
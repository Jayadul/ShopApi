using Shop.Application.Common.Models;
using Shop.Domain.Entities;

namespace Shop.Application.Common.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<PagedResult<Product>> GetAllAsync(PaginationParams paginationParams, CancellationToken ct = default);
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

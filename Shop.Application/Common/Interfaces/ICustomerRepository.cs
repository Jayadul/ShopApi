using Shop.Application.Common.Models;
using Shop.Domain.Entities;

namespace Shop.Application.Common.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<Customer>> GetAllAsync(PaginationParams paginationParams, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default);
    void Add(Customer customer);
    void Update(Customer customer);
    void Delete(Customer customer);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

using Shop.Application.Common.Models;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Common.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<Order>> GetAllAsync(
        PaginationParams paginationParams,
        int? customerId,
        DateTime? fromDate,
        DateTime? toDate,
        OrderStatus? status,
        CancellationToken ct = default);
    void Add(Order order);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

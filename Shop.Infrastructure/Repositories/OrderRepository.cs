using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Infrastructure.Persistence;

namespace Shop.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // Single query with all related data via JOINs - no N+1
        return await _db.Orders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<PagedResult<Order>> GetAllAsync(
        PaginationParams paginationParams,
        int? customerId,
        DateTime? fromDate,
        DateTime? toDate,
        OrderStatus? status,
        CancellationToken ct = default)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(x => x.CustomerId == customerId.Value);

        if (fromDate.HasValue)
            query = query.Where(x => x.CreationDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.CreationDate <= toDate.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(paginationParams.SearchBy))
        {
            var search = paginationParams.SearchBy.ToLower();
            query = query.Where(x => x.OrderNumber.ToLower().Contains(search));
        }

        query = paginationParams.OrderPropertyName?.ToLower() switch
        {
            "totalamount" => paginationParams.SortOrder == "desc" ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
            "status" => paginationParams.SortOrder == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => query.OrderByDescending(x => x.CreationDate)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize
        };
    }

    public void Add(Order order) => _db.Orders.Add(order);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}

using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Domain.Entities;
using Shop.Infrastructure.Persistence;

namespace Shop.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<PagedResult<Customer>> GetAllAsync(PaginationParams paginationParams, CancellationToken ct = default)
    {
        var query = _db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(paginationParams.SearchBy))
        {
            var search = paginationParams.SearchBy.ToLower();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(search) ||
                x.Email.ToLower().Contains(search));
        }

        // Sorting
        query = paginationParams.OrderPropertyName?.ToLower() switch
        {
            "fullname" => paginationParams.SortOrder == "desc" ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "email" => paginationParams.SortOrder == "desc" ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            _ => query.OrderByDescending(x => x.CreationDate)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Customer>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Customers.Where(x => x.Email == email);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public void Add(Customer customer) => _db.Customers.Add(customer);

    public void Update(Customer customer)
    {
        _db.Customers.Attach(customer);
        _db.Entry(customer).State = EntityState.Modified;
    }

    public void Delete(Customer customer) => _db.Customers.Remove(customer);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}

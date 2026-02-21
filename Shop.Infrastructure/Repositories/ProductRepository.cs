using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Domain.Entities;
using Shop.Infrastructure.Persistence;

namespace Shop.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        => await _db.Products.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync(ct);

    public async Task<PagedResult<Product>> GetAllAsync(PaginationParams paginationParams, CancellationToken ct = default)
    {
        var query = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(paginationParams.SearchBy))
        {
            var search = paginationParams.SearchBy.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || x.Description.ToLower().Contains(search));
        }

        query = paginationParams.OrderPropertyName?.ToLower() switch
        {
            "price" => paginationParams.SortOrder == "desc" ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
            "name"  => paginationParams.SortOrder == "desc" ? query.OrderByDescending(x => x.Name)  : query.OrderBy(x => x.Name),
            _ => query.OrderByDescending(x => x.CreationDate)
        };

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize
        };
    }

    public void Add(Product product) => _db.Products.Add(product);

    public void Update(Product product)
    {
        _db.Products.Attach(product);
        _db.Entry(product).State = EntityState.Modified;
    }

    public void Delete(Product product) => _db.Products.Remove(product);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}

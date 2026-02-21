using Shop.Domain.Entities;

namespace Shop.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    void Add(User user);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

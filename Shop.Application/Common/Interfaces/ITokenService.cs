using Shop.Domain.Entities;

namespace Shop.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}

using Nstech.OrderService.Domain.Entities;

namespace Nstech.OrderService.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken);
}

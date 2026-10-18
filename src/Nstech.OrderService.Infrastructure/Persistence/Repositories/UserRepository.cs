using Microsoft.EntityFrameworkCore;
using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Domain.Entities;

namespace Nstech.OrderService.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _db.Users.FirstOrDefaultAsync(user => user.Email == normalized, cancellationToken);
    }
}

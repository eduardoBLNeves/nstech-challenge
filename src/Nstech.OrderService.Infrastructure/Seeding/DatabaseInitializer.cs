using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Infrastructure.Persistence;

namespace Nstech.OrderService.Infrastructure.Seeding;

public static class DatabaseInitializer
{
    public static async Task Apply(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwords = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.MigrateAsync(cancellationToken);
        await DbSeeder.Seed(db, passwords, cancellationToken);
    }
}

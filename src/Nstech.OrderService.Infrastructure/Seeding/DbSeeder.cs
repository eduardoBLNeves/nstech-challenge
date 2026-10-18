using Microsoft.EntityFrameworkCore;
using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Enums;
using Nstech.OrderService.Infrastructure.Persistence;

namespace Nstech.OrderService.Infrastructure.Seeding;

public static class DbSeeder
{
    public static readonly Guid NotebookId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid MouseId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid KeyboardId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public const string UserEmail = "user@example.com";
    public const string UserPassword = "12345678";

    public static async Task Seed(AppDbContext db, IPasswordHasher passwords, CancellationToken cancellationToken)
    {
        if (!await db.Users.AnyAsync(user => user.Email == UserEmail, cancellationToken))
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = UserEmail,
                PasswordHash = passwords.Hash(UserPassword),
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Products.AnyAsync(cancellationToken))
        {
            db.Products.AddRange(
                new Product { Id = NotebookId, Name = "Notebook", UnitPrice = 3500m, AvailableQuantity = 20 },
                new Product { Id = MouseId, Name = "Mouse", UnitPrice = 80m, AvailableQuantity = 100 },
                new Product { Id = KeyboardId, Name = "Keyboard", UnitPrice = 220m, AvailableQuantity = 50 });

            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.Orders.AnyAsync(cancellationToken))
        {
            var customerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            db.Orders.AddRange(
                new Order
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01"),
                    CustomerId = customerId,
                    Status = OrderStatus.Placed,
                    Currency = "BRL",
                    Total = 3500m,
                    CreatedAt = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc),
                    Items =
                    [
                        new OrderItem
                        {
                            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01"),
                            ProductId = NotebookId,
                            UnitPrice = 3500m,
                            Quantity = 1
                        }
                    ]
                },
                new Order
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02"),
                    CustomerId = customerId,
                    Status = OrderStatus.Placed,
                    Currency = "BRL",
                    Total = 80m,
                    CreatedAt = new DateTime(2026, 10, 15, 13, 0, 0, DateTimeKind.Utc),
                    Items =
                    [
                        new OrderItem
                        {
                            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02"),
                            ProductId = MouseId,
                            UnitPrice = 80m,
                            Quantity = 1
                        }
                    ]
                });

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}

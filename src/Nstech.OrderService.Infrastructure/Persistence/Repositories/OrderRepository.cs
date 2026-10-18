using Microsoft.EntityFrameworkCore;
using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Application.Orders;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Enums;

namespace Nstech.OrderService.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task Add(Order order, CancellationToken cancellationToken)
    {
        await _db.Orders.AddAsync(order, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(Order order, CancellationToken cancellationToken)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdAsNotracking(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<(List<Order> Items, int Total)> List(ListOrdersRequest request, CancellationToken cancellationToken)
    {
        var query = _db.Orders.AsNoTracking().Include(order => order.Items).AsQueryable();

        if (request.CustomerId is Guid customerId)
            query = query.Where(order => order.CustomerId == customerId);

        if (request.Status is OrderStatus status)
            query = query.Where(order => order.Status == status);

        if (request.From is DateTime from)
            query = query.Where(order => order.CreatedAt >= from);

        if (request.To is DateTime to)
            query = query.Where(order => order.CreatedAt <= to);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(order => order.CreatedAt)
            .ThenByDescending(order => order.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}

using Microsoft.EntityFrameworkCore;
using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Application.Products;
using Nstech.OrderService.Domain.Entities;

namespace Nstech.OrderService.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task Add(Product product, CancellationToken cancellationToken)
    {
        await _db.Products.AddAsync(product, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(Product product, CancellationToken cancellationToken)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task Delete(Product product, CancellationToken cancellationToken)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<Product?> GetByIdAsNotracking(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<List<Product>> GetByIds(List<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return new List<Product>();

        return await _db.Products
            .Where(product => ids.Contains(product.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Product> Items, int Total)> List(ListProductsRequest request, CancellationToken cancellationToken)
    {
        var query = _db.Products.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> IsUsedInOrders(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Set<OrderItem>().AnyAsync(item => item.ProductId == id, cancellationToken);
    }
}

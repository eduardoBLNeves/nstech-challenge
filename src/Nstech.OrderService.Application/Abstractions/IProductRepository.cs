using Nstech.OrderService.Application.Products;
using Nstech.OrderService.Domain.Entities;

namespace Nstech.OrderService.Application.Abstractions;

public interface IProductRepository
{
    Task Add(Product product, CancellationToken cancellationToken);
    Task Update(Product product, CancellationToken cancellationToken);
    Task Delete(Product product, CancellationToken cancellationToken);
    Task<Product?> GetById(Guid id, CancellationToken cancellationToken);
    Task<Product?> GetByIdAsNotracking(Guid id, CancellationToken cancellationToken);
    Task<List<Product>> GetByIds(List<Guid> ids, CancellationToken cancellationToken);
    Task<(List<Product> Items, int Total)> List(ListProductsRequest request, CancellationToken cancellationToken);
    Task<bool> IsUsedInOrders(Guid id, CancellationToken cancellationToken);
}

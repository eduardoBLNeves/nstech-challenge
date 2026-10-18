using Nstech.OrderService.Application.Orders;
using Nstech.OrderService.Domain.Entities;

namespace Nstech.OrderService.Application.Abstractions;

public interface IOrderRepository
{
    Task Add(Order order, CancellationToken cancellationToken);
    Task Update(Order order, CancellationToken cancellationToken);
    Task<Order?> GetById(Guid id, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsNotracking(Guid id, CancellationToken cancellationToken);
    Task<(List<Order> Items, int Total)> List(ListOrdersRequest request, CancellationToken cancellationToken);
}

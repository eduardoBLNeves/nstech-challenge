using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Domain;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Enums;
using Nstech.OrderService.Domain.Exceptions;

namespace Nstech.OrderService.Application.Orders;

public class OrderAppService
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;

    public OrderAppService(IOrderRepository orders, IProductRepository products)
    {
        _orders = orders;
        _products = products;
    }

    public async Task<OrderResponse> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var ids = request.Items.Select(item => item.ProductId).Distinct().ToList();
        var products = await _products.GetByIds(ids, cancellationToken);
        if (products.Count != ids.Count)
            throw new AppException(Messages.ProductNotFound);

        foreach (var item in request.Items)
        {
            var product = products.First(current => current.Id == item.ProductId);
            if (product.AvailableQuantity < item.Quantity)
                throw new AppException(Messages.InsufficientStock);
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Status = OrderStatus.Placed,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(item =>
            {
                var product = products.First(current => current.Id == item.ProductId);
                return new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    UnitPrice = product.UnitPrice,
                    Quantity = item.Quantity
                };
            }).ToList()
        };

        order.Total = order.Items.Sum(item => item.UnitPrice * item.Quantity);
        await _orders.Add(order, cancellationToken);
        return ToResponse(order);
    }

    public async Task<OrderResponse> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetByIdAsNotracking(id, cancellationToken)
            ?? throw new AppException(Messages.OrderNotFound, 404);

        return ToResponse(order);
    }

    public async Task<PagedResult> List(ListOrdersRequest request, CancellationToken cancellationToken)
    {
        request.From = NormalizeUtc(request.From);
        request.To = NormalizeUtc(request.To);

        var (items, total) = await _orders.List(request, cancellationToken);

        return new PagedResult(items.Select(ToResponse).ToList(), request.Page, request.PageSize, total);
    }

    public async Task<OrderResponse> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetById(id, cancellationToken)
            ?? throw new AppException(Messages.OrderNotFound, 404);

        if (order.Status == OrderStatus.Confirmed)
            return ToResponse(order);

        if (order.Status != OrderStatus.Placed)
            throw new AppException(Messages.CantUpdateOrderStatus, 409);

        await AdjustStock(order, release: false, cancellationToken);
        order.Status = OrderStatus.Confirmed;
        await _orders.Update(order, cancellationToken);
        return ToResponse(order);
    }

    public async Task<OrderResponse> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetById(id, cancellationToken)
            ?? throw new AppException(Messages.OrderNotFound, 404);

        if (order.Status == OrderStatus.Canceled)
            return ToResponse(order);

        if (order.Status == OrderStatus.Confirmed)
            await AdjustStock(order, release: true, cancellationToken);
        else if (order.Status != OrderStatus.Placed)
            throw new AppException(Messages.CantUpdateOrderStatus, 409);

        order.Status = OrderStatus.Canceled;
        await _orders.Update(order, cancellationToken);
        return ToResponse(order);
    }

    private async Task AdjustStock(Order order, bool release, CancellationToken cancellationToken)
    {
        var ids = order.Items.Select(item => item.ProductId).Distinct().ToList();
        var products = await _products.GetByIds(ids, cancellationToken);
        if (products.Count != ids.Count)
            throw new AppException(Messages.ProductNotFound);

        foreach (var item in order.Items)
        {
            var product = products.First(current => current.Id == item.ProductId);
            if (release)
                product.AvailableQuantity += item.Quantity;
            else
            {
                if (product.AvailableQuantity < item.Quantity)
                    throw new AppException(Messages.InsufficientStock);

                product.AvailableQuantity -= item.Quantity;
            }
        }
    }

    private static DateTime? NormalizeUtc(DateTime? value)
    {
        if (value is null)
            return null;

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }

    private static OrderResponse ToResponse(Order order)
    {
        var items = order.Items
            .Select(item => new OrderItemResponse(item.ProductId, item.UnitPrice, item.Quantity))
            .ToList();

        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.Currency,
            order.Total,
            order.CreatedAt,
            items);
    }
}

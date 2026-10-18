using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Application.Orders;
using Nstech.OrderService.Application.Products;
using Nstech.OrderService.Domain;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Enums;
using Nstech.OrderService.Domain.Exceptions;

namespace Nstech.OrderService.Application.Tests;

public class OrderAppServiceTests
{
    [Fact]
    public async Task Create_saves_order_without_decreasing_stock()
    {
        var product = SampleProduct(stock: 5);
        var orders = new FakeOrderRepository();
        var service = new OrderAppService(orders, new FakeProductRepository(product));

        var response = await service.Create(OrderRequest(product.Id, quantity: 2), CancellationToken.None);

        Assert.Equal(OrderStatus.Placed.ToString(), response.Status);
        Assert.Equal("BRL", response.Currency);
        Assert.Equal(20, response.Total);
        Assert.Equal(5, product.AvailableQuantity);
        Assert.Single(orders.Orders);
    }

    [Theory]
    [InlineData(false, 1, Messages.ProductNotFound)]
    [InlineData(true, 99, Messages.InsufficientStock)]
    public async Task Create_rejects_missing_product_or_stock(bool productExists, int quantity, string message)
    {
        var product = SampleProduct(stock: 5);
        var products = productExists ? new FakeProductRepository(product) : new FakeProductRepository();
        var service = new OrderAppService(new FakeOrderRepository(), products);

        var error = await Assert.ThrowsAsync<AppException>(() =>
            service.Create(OrderRequest(product.Id, quantity), CancellationToken.None));

        Assert.Equal(message, error.Message);
    }

    [Fact]
    public async Task GetById_returns_404_when_missing()
    {
        var service = new OrderAppService(new FakeOrderRepository(), new FakeProductRepository());

        var error = await Assert.ThrowsAsync<AppException>(() =>
            service.GetById(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(404, error.StatusCode);
        Assert.Equal(Messages.OrderNotFound, error.Message);
    }

    [Fact]
    public async Task Confirm_decreases_stock()
    {
        var product = SampleProduct(stock: 5);
        var order = SavedOrder(product, OrderStatus.Placed, quantity: 2);
        var service = Service(product, order);

        var response = await service.Confirm(order.Id, CancellationToken.None);

        Assert.Equal(OrderStatus.Confirmed.ToString(), response.Status);
        Assert.Equal(3, product.AvailableQuantity);
    }

    [Fact]
    public async Task Confirm_again_does_not_decrease_stock()
    {
        var product = SampleProduct(stock: 5);
        var order = SavedOrder(product, OrderStatus.Placed, quantity: 2);
        var service = Service(product, order);

        await service.Confirm(order.Id, CancellationToken.None);
        await service.Confirm(order.Id, CancellationToken.None);

        Assert.Equal(3, product.AvailableQuantity);
    }

    [Theory]
    [InlineData(OrderStatus.Canceled)]
    [InlineData(OrderStatus.Draft)]
    public async Task Confirm_rejects_invalid_status(OrderStatus status)
    {
        var product = SampleProduct(stock: 5);
        var order = SavedOrder(product, status, quantity: 2);
        var service = Service(product, order);

        var error = await Assert.ThrowsAsync<AppException>(() =>
            service.Confirm(order.Id, CancellationToken.None));

        Assert.Equal(409, error.StatusCode);
        Assert.Equal(Messages.CantUpdateOrderStatus, error.Message);
        Assert.Equal(5, product.AvailableQuantity);
    }

    [Fact]
    public async Task Cancel_placed_does_not_change_stock()
    {
        var product = SampleProduct(stock: 5);
        var order = SavedOrder(product, OrderStatus.Placed, quantity: 2);
        var service = Service(product, order);

        var response = await service.Cancel(order.Id, CancellationToken.None);

        Assert.Equal(OrderStatus.Canceled.ToString(), response.Status);
        Assert.Equal(5, product.AvailableQuantity);
    }

    [Fact]
    public async Task Cancel_confirmed_restores_stock()
    {
        var product = SampleProduct(stock: 5);
        var order = SavedOrder(product, OrderStatus.Confirmed, quantity: 2);
        var service = Service(product, order);

        await service.Cancel(order.Id, CancellationToken.None);

        Assert.Equal(7, product.AvailableQuantity);
    }

    [Fact]
    public async Task Cancel_again_does_not_change_stock()
    {
        var product = SampleProduct(stock: 5);
        var order = SavedOrder(product, OrderStatus.Confirmed, quantity: 2);
        var service = Service(product, order);

        await service.Cancel(order.Id, CancellationToken.None);
        await service.Cancel(order.Id, CancellationToken.None);

        Assert.Equal(7, product.AvailableQuantity);
    }

    [Fact]
    public async Task Cancel_draft_returns_409()
    {
        var product = SampleProduct(stock: 5);
        var order = SavedOrder(product, OrderStatus.Draft, quantity: 2);
        var service = Service(product, order);

        var error = await Assert.ThrowsAsync<AppException>(() =>
            service.Cancel(order.Id, CancellationToken.None));

        Assert.Equal(409, error.StatusCode);
        Assert.Equal(5, product.AvailableQuantity);
    }

    private static OrderAppService Service(Product product, Order order)
    {
        return new OrderAppService(new FakeOrderRepository(order), new FakeProductRepository(product));
    }

    private static Product SampleProduct(int stock)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = "Mouse",
            UnitPrice = 10,
            AvailableQuantity = stock
        };
    }

    private static CreateOrderRequest OrderRequest(Guid productId, int quantity)
    {
        return new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Currency = "brl",
            Items = [new CreateOrderItemRequest { ProductId = productId, Quantity = quantity }]
        };
    }

    private static Order SavedOrder(Product product, OrderStatus status, int quantity)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = status,
            Currency = "BRL",
            CreatedAt = DateTime.UtcNow,
            Total = product.UnitPrice * quantity,
            Items =
            [
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    UnitPrice = product.UnitPrice,
                    Quantity = quantity
                }
            ]
        };
    }

    private class FakeOrderRepository : IOrderRepository
    {
        public List<Order> Orders { get; } = [];

        public FakeOrderRepository(params Order[] orders)
        {
            Orders.AddRange(orders);
        }

        public Task Add(Order order, CancellationToken cancellationToken)
        {
            Orders.Add(order);
            return Task.CompletedTask;
        }

        public Task Update(Order order, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<Order?> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Orders.FirstOrDefault(order => order.Id == id));
        }

        public Task<Order?> GetByIdAsNotracking(Guid id, CancellationToken cancellationToken)
        {
            return GetById(id, cancellationToken);
        }

        public Task<(List<Order> Items, int Total)> List(ListOrdersRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult((Orders, Orders.Count));
        }
    }

    private class FakeProductRepository : IProductRepository
    {
        private readonly List<Product> _products;

        public FakeProductRepository(params Product[] products)
        {
            _products = products.ToList();
        }

        public Task<List<Product>> GetByIds(List<Guid> ids, CancellationToken cancellationToken)
        {
            var found = _products.Where(product => ids.Contains(product.Id)).ToList();
            return Task.FromResult(found);
        }

        public Task Add(Product product, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task Update(Product product, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task Delete(Product product, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<Product?> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_products.FirstOrDefault(product => product.Id == id));
        }

        public Task<Product?> GetByIdAsNotracking(Guid id, CancellationToken cancellationToken)
        {
            return GetById(id, cancellationToken);
        }

        public Task<(List<Product> Items, int Total)> List(ListProductsRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult((_products, _products.Count));
        }

        public Task<bool> IsUsedInOrders(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }
}

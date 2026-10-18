using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Application.Products;
using Nstech.OrderService.Domain;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Exceptions;

namespace Nstech.OrderService.Application.Tests;

public class ProductAppServiceTests
{
    [Fact]
    public async Task Create_saves_product()
    {
        var products = new FakeProductRepository();
        var service = new ProductAppService(products);

        var response = await service.Create(ProductRequest("  Mouse  ", 80, 10), CancellationToken.None);

        Assert.Equal("Mouse", response.Name);
        Assert.Equal(80, response.UnitPrice);
        Assert.Equal(10, response.AvailableQuantity);
        Assert.Single(products.Products);
    }

    [Fact]
    public async Task GetById_returns_404_when_missing()
    {
        var service = new ProductAppService(new FakeProductRepository());

        var error = await Assert.ThrowsAsync<AppException>(() =>
            service.GetById(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(404, error.StatusCode);
        Assert.Equal(Messages.ProductNotFound, error.Message);
    }

    [Fact]
    public async Task Update_changes_name_price_and_stock()
    {
        var product = SampleProduct();
        var service = new ProductAppService(new FakeProductRepository(product));

        var response = await service.Update(product.Id, ProductUpdate("Keyboard", 220, 4), CancellationToken.None);

        Assert.Equal("Keyboard", response.Name);
        Assert.Equal(220, response.UnitPrice);
        Assert.Equal(4, response.AvailableQuantity);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal(4, product.AvailableQuantity);
    }

    [Fact]
    public async Task Delete_removes_product()
    {
        var product = SampleProduct();
        var products = new FakeProductRepository(product);
        var service = new ProductAppService(products);

        var response = await service.Delete(product.Id, CancellationToken.None);

        Assert.Equal(product.Id, response.Id);
        Assert.Empty(products.Products);
    }

    [Theory]
    [InlineData(false, false, 404, Messages.ProductNotFound)]
    [InlineData(true, true, 409, Messages.CantDeleteProduct)]
    public async Task Delete_rejects_missing_or_used_product(bool exists, bool usedInOrders, int statusCode, string message)
    {
        var product = SampleProduct();
        var products = exists
            ? new FakeProductRepository(product) { UsedInOrders = usedInOrders }
            : new FakeProductRepository { UsedInOrders = usedInOrders };
        var service = new ProductAppService(products);

        var error = await Assert.ThrowsAsync<AppException>(() =>
            service.Delete(product.Id, CancellationToken.None));

        Assert.Equal(statusCode, error.StatusCode);
        Assert.Equal(message, error.Message);
    }

    private static Product SampleProduct()
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = "Mouse",
            UnitPrice = 80,
            AvailableQuantity = 10
        };
    }

    private static CreateProductRequest ProductRequest(string name, decimal price, int quantity)
    {
        return new CreateProductRequest
        {
            Name = name,
            UnitPrice = price,
            AvailableQuantity = quantity
        };
    }

    private static UpdateProductRequest ProductUpdate(string name, decimal price, int quantity)
    {
        return new UpdateProductRequest
        {
            Name = name,
            UnitPrice = price,
            AvailableQuantity = quantity
        };
    }

    private class FakeProductRepository : IProductRepository
    {
        public List<Product> Products { get; } = [];
        public bool UsedInOrders { get; set; }

        public FakeProductRepository(params Product[] products)
        {
            Products.AddRange(products);
        }

        public Task Add(Product product, CancellationToken cancellationToken)
        {
            Products.Add(product);
            return Task.CompletedTask;
        }

        public Task Update(Product product, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task Delete(Product product, CancellationToken cancellationToken)
        {
            Products.Remove(product);
            return Task.CompletedTask;
        }

        public Task<Product?> GetById(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Products.FirstOrDefault(product => product.Id == id));
        }

        public Task<Product?> GetByIdAsNotracking(Guid id, CancellationToken cancellationToken)
        {
            return GetById(id, cancellationToken);
        }

        public Task<List<Product>> GetByIds(List<Guid> ids, CancellationToken cancellationToken)
        {
            var found = Products.Where(product => ids.Contains(product.Id)).ToList();
            return Task.FromResult(found);
        }

        public Task<(List<Product> Items, int Total)> List(ListProductsRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult((Products, Products.Count));
        }

        public Task<bool> IsUsedInOrders(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(UsedInOrders);
        }
    }
}

using Nstech.OrderService.Application.Abstractions;
using Nstech.OrderService.Domain;
using Nstech.OrderService.Domain.Entities;
using Nstech.OrderService.Domain.Exceptions;

namespace Nstech.OrderService.Application.Products;

public class ProductAppService
{
    private readonly IProductRepository _products;

    public ProductAppService(IProductRepository products)
    {
        _products = products;
    }

    public async Task<ProductResponse> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            UnitPrice = request.UnitPrice,
            AvailableQuantity = request.AvailableQuantity
        };

        await _products.Add(product, cancellationToken);
        return ToResponse(product);
    }

    public async Task<ProductResponse> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsNotracking(id, cancellationToken)
            ?? throw new AppException(Messages.ProductNotFound, 404);

        return ToResponse(product);
    }

    public async Task<ProductPagedResult> List(ListProductsRequest request, CancellationToken cancellationToken)
    {
        var (items, total) = await _products.List(request, cancellationToken);
        return new ProductPagedResult(items.Select(ToResponse).ToList(), request.Page, request.PageSize, total);
    }

    public async Task<ProductResponse> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _products.GetById(id, cancellationToken)
            ?? throw new AppException(Messages.ProductNotFound, 404);

        product.Name = request.Name.Trim();
        product.UnitPrice = request.UnitPrice;
        product.AvailableQuantity = request.AvailableQuantity;
        await _products.Update(product, cancellationToken);
        return ToResponse(product);
    }

    public async Task<ProductResponse> Delete(Guid id, CancellationToken cancellationToken)
    {
        var product = await _products.GetById(id, cancellationToken)
            ?? throw new AppException(Messages.ProductNotFound, 404);

        if (await _products.IsUsedInOrders(id, cancellationToken))
            throw new AppException(Messages.CantDeleteProduct, 409);

        await _products.Delete(product, cancellationToken);
        return ToResponse(product);
    }

    private static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse(product.Id, product.Name, product.UnitPrice, product.AvailableQuantity);
    }
}

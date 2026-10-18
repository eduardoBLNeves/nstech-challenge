using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nstech.OrderService.Api;
using Nstech.OrderService.Application.Products;

namespace Nstech.OrderService.Api.Controllers;

[ApiController]
[Authorize]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly ProductAppService _products;

    public ProductsController(ProductAppService products)
    {
        _products = products;
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<ProductResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _products.Create(request, cancellationToken);
        var response = ApiResponse<ProductResponse>.Ok(product, StatusCodes.Status201Created);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _products.Update(id, request, cancellationToken);
        return Ok(ApiResponse<ProductResponse>.Ok(product));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var product = await _products.Delete(id, cancellationToken);
        return Ok(ApiResponse<ProductResponse>.Ok(product));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _products.GetById(id, cancellationToken);
        return Ok(ApiResponse<ProductResponse>.Ok(product));
    }

    [HttpGet]
    [ProducesResponseType<ApiResponse<ProductPagedResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ProductPagedResult>>> List(
        [FromQuery] ListProductsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _products.List(request, cancellationToken);
        return Ok(ApiResponse<ProductPagedResult>.Ok(result));
    }
}

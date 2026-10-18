using Microsoft.AspNetCore.Authorization;
using Nstech.OrderService.Api;
using Microsoft.AspNetCore.Mvc;
using Nstech.OrderService.Application.Orders;

namespace Nstech.OrderService.Api.Controllers;

[ApiController]
[Authorize]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderAppService _orders;

    public OrdersController(OrderAppService orders)
    {
        _orders = orders;
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<OrderResponse>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orders.Create(request, cancellationToken);
        var response = ApiResponse<OrderResponse>.Ok(order, StatusCodes.Status201Created);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, response);
    }

    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType<ApiResponse<OrderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orders.Confirm(id, cancellationToken);
        return Ok(ApiResponse<OrderResponse>.Ok(order));
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<ApiResponse<OrderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orders.Cancel(id, cancellationToken);
        return Ok(ApiResponse<OrderResponse>.Ok(order));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<OrderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetById(id, cancellationToken);
        return Ok(ApiResponse<OrderResponse>.Ok(order));
    }

    [HttpGet]
    [ProducesResponseType<ApiResponse<PagedResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult>>> List(
        [FromQuery] ListOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orders.List(request, cancellationToken);
        return Ok(ApiResponse<PagedResult>.Ok(result));
    }
}

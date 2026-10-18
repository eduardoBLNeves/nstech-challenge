using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Nstech.OrderService.Domain;
using Nstech.OrderService.Domain.Enums;

namespace Nstech.OrderService.Application.Orders;

public class CreateOrderRequest : IValidatableObject
{
    [Required(ErrorMessage = Messages.ValidationError)]
    public Guid CustomerId { get; set; }

    [Required(ErrorMessage = Messages.ValidationError)]
    [RegularExpression("^[A-Za-z]{3}$", ErrorMessage = Messages.ValidationError)]
    public string Currency { get; set; } = string.Empty;

    [Required(ErrorMessage = Messages.ValidationError)]
    [MinLength(1, ErrorMessage = Messages.ValidationError)]
    public List<CreateOrderItemRequest> Items { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CustomerId == Guid.Empty)
            yield return new ValidationResult(Messages.ValidationError, [nameof(CustomerId)]);

        if (Items.Any(item => item.ProductId == Guid.Empty))
            yield return new ValidationResult(Messages.ValidationError, [nameof(Items)]);

        if (Items.Select(item => item.ProductId).Distinct().Count() != Items.Count)
            yield return new ValidationResult(Messages.ValidationError, [nameof(Items)]);
    }
}

public class CreateOrderItemRequest
{
    [Required(ErrorMessage = Messages.ValidationError)]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = Messages.ValidationError)]
    public int Quantity { get; set; }
}

public class ListOrdersRequest : IValidatableObject
{
    public Guid? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    [DefaultValue(1)]
    [Range(1, int.MaxValue, ErrorMessage = Messages.ValidationError)]
    public int Page { get; set; } = 1;

    [DefaultValue(20)]
    [Range(1, 100, ErrorMessage = Messages.ValidationError)]
    public int PageSize { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From is not null && To is not null && From > To)
            yield return new ValidationResult(Messages.ValidationError, [nameof(From), nameof(To)]);
    }
}

public record OrderItemResponse(Guid ProductId, decimal UnitPrice, int Quantity);

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    string Currency,
    decimal Total,
    DateTime CreatedAt,
    List<OrderItemResponse> Items);

public record PagedResult(List<OrderResponse> Items, int Page, int PageSize, int Total);

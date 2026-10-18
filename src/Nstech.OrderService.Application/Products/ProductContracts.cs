using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Nstech.OrderService.Domain;

namespace Nstech.OrderService.Application.Products;

public class CreateProductRequest : IValidatableObject
{
    [Required(ErrorMessage = Messages.ValidationError)]
    [MaxLength(120, ErrorMessage = Messages.ValidationError)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = Messages.ValidationError)]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = Messages.ValidationError)]
    public int AvailableQuantity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
            yield return new ValidationResult(Messages.ValidationError, [nameof(Name)]);
    }
}

public class UpdateProductRequest : IValidatableObject
{
    [Required(ErrorMessage = Messages.ValidationError)]
    [MaxLength(120, ErrorMessage = Messages.ValidationError)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = Messages.ValidationError)]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = Messages.ValidationError)]
    public int AvailableQuantity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
            yield return new ValidationResult(Messages.ValidationError, [nameof(Name)]);
    }
}

public class ListProductsRequest
{
    [DefaultValue(1)]
    [Range(1, int.MaxValue, ErrorMessage = Messages.ValidationError)]
    public int Page { get; set; } = 1;

    [DefaultValue(20)]
    [Range(1, 100, ErrorMessage = Messages.ValidationError)]
    public int PageSize { get; set; } = 20;
}

public record ProductResponse(Guid Id, string Name, decimal UnitPrice, int AvailableQuantity);

public record ProductPagedResult(List<ProductResponse> Items, int Page, int PageSize, int Total);

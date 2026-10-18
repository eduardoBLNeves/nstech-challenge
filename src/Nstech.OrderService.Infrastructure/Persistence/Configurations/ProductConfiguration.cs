using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nstech.OrderService.Domain.Entities;

namespace Nstech.OrderService.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Name).HasMaxLength(120).IsRequired();
        builder.Property(product => product.UnitPrice).HasPrecision(18, 2);
    }
}

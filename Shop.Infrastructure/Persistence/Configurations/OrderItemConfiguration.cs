using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();

        // Ignore computed property - not stored in DB
        builder.Ignore(x => x.LineTotal);

        // Index for loading items of an order (prevents N+1)
        builder.HasIndex(x => x.OrderId).HasDatabaseName("IX_OrderItems_OrderId");
        builder.HasIndex(x => x.ProductId).HasDatabaseName("IX_OrderItems_ProductId");

        builder.HasOne(x => x.Order)
               .WithMany(x => x.OrderItems)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
               .WithMany(x => x.OrderItems)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsArchived);
    }
}

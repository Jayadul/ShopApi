using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        // Composite index for filtering by date range and status (prevents full table scans)
        builder.HasIndex(x => new { x.Status, x.CreationDate }).HasDatabaseName("IX_Orders_Status_CreationDate");

        // Index for customer order lookups (prevents N+1)
        builder.HasIndex(x => x.CustomerId).HasDatabaseName("IX_Orders_CustomerId");

        builder.HasIndex(x => x.IsArchived).HasDatabaseName("IX_Orders_IsArchived");

        // OrderItems are configured in OrderItemConfiguration (cascade delete)
        builder.HasMany(x => x.OrderItems)
               .WithOne(x => x.Order)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsArchived);
    }
}

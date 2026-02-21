using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Phone).IsRequired().HasMaxLength(30);
        builder.Property(x => x.Address).IsRequired().HasMaxLength(500);

        // Index for email lookups (GDPR-aware unique constraint)
        builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("IX_Customers_Email");

        // Index for filtering non-archived customers
        builder.HasIndex(x => x.IsArchived).HasDatabaseName("IX_Customers_IsArchived");

        builder.HasMany(x => x.Orders)
               .WithOne(x => x.Customer)
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        // Global query filter: exclude archived (soft-deleted) records
        builder.HasQueryFilter(x => !x.IsArchived);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(250);
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.PasswordSalt).IsRequired();

        // Store enum as string so the column is readable in DB (e.g. "Admin" not "2")
        builder.Property(x => x.Role)
               .IsRequired()
               .HasMaxLength(50)
               .HasConversion<string>();

        builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("IX_Users_Email");
    }
}

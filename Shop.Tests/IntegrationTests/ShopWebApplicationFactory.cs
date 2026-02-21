using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shop.Infrastructure.Persistence;

namespace Shop.Tests.IntegrationTests;

public class ShopWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 1. Configure JWT Settings (Must match SHA512 requirements: 64+ chars)
        builder.UseSetting("JwtSettings:Key", "ShopApi_SuperSecretKey_MustBeAtLeast64CharactersLongForHmacSha512Algorithm!!");
        builder.UseSetting("JwtSettings:Issuer", "ShopApi");
        builder.UseSetting("JwtSettings:Audience", "ShopApiClients");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "60");

        builder.ConfigureServices(services =>
        {
            // 2. Remove the production DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // 3. Register In-Memory Database with a unique name per test run
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestShopDb_" + Guid.NewGuid()));

            // 4. Seed Data using the existing DbSeeder
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure tables exist (In-Memory does this automatically, but explicit is safe)
            db.Database.EnsureCreated();

            // Call seeder with FALSE to skip Migrate() which crashes on In-Memory
            DbSeeder.SeedAsync(db, runMigrations: false).GetAwaiter().GetResult();
        });
    }
}
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace Shop.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedAsync(db, runMigrations: true);
    }

    public static async Task SeedAsync(AppDbContext db, bool runMigrations)
    {
        if (runMigrations && db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }

        if (await db.Users.AnyAsync()) return;

        await SeedUsersAsync(db);
        await SeedCustomersAsync(db);
        await SeedProductsAsync(db);
        await SeedOrdersAsync(db);
    }

    private static async Task SeedUsersAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        db.Users.AddRange(
            CreateUser("admin", "admin@shop.com", "Admin123!", UserRole.Admin),
            CreateUser("john_doe", "john@customer.com", "Customer123!", UserRole.Customer),
            CreateUser("jane_doe", "jane@customer.com", "Customer123!", UserRole.Customer)
        );

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a User with a correctly paired HMACSHA512 password hash and salt.
    ///
    /// Key rule: salt (hmac.Key) MUST be captured BEFORE calling ComputeHash.
    /// HMACSHA512 can internally mutate its Key buffer during hashing.
    /// If you read hmac.Key after ComputeHash, you may get a different value
    /// than what was actually used — causing all logins to return 401.
    ///
    /// LoginHandler verifies with: new HMACSHA512(Convert.FromBase64String(user.PasswordSalt))
    /// So the stored salt must be the exact key used when the hash was computed.
    /// </summary>
    private static User CreateUser(string username, string email, string password, UserRole role)
    {
        using var hmac = new HMACSHA512();

        // Step 1: Capture salt BEFORE hashing
        var passwordSalt = Convert.ToBase64String(hmac.Key);

        // Step 2: Hash using the same instance (key is stable at this point)
        var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = role,
            CreationDate = DateTime.UtcNow
        };
    }

    private static async Task SeedCustomersAsync(AppDbContext db)
    {
        if (await db.Customers.AnyAsync()) return;

        db.Customers.AddRange(
            new Customer
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Phone = "+1-555-0101",
                Address = "123 Elm Street, New York, NY 10001",
                CreationDate = DateTime.UtcNow
            },
            new Customer
            {
                FullName = "Jane Smith",
                Email = "jane.smith@example.com",
                Phone = "+1-555-0102",
                Address = "456 Oak Avenue, Los Angeles, CA 90001",
                CreationDate = DateTime.UtcNow
            },
            new Customer
            {
                FullName = "Bob Johnson",
                Email = "bob.johnson@example.com",
                Phone = "+1-555-0103",
                Address = "789 Pine Road, Chicago, IL 60601",
                CreationDate = DateTime.UtcNow
            }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        db.Products.AddRange(
            new Product
            {
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with 2.4GHz connectivity, 1600 DPI, and 18-month battery life.",
                Price = 29.99m,
                Stock = 150,
                CreationDate = DateTime.UtcNow
            },
            new Product
            {
                Name = "Mechanical Keyboard",
                Description = "Full-size mechanical keyboard with Cherry MX Blue switches, RGB backlight, and USB-C.",
                Price = 89.99m,
                Stock = 80,
                CreationDate = DateTime.UtcNow
            },
            new Product
            {
                Name = "USB-C Hub",
                Description = "7-in-1 USB-C hub with HDMI 4K, 3x USB 3.0, SD card reader, and 100W PD charging.",
                Price = 49.99m,
                Stock = 200,
                CreationDate = DateTime.UtcNow
            },
            new Product
            {
                Name = "27\" 4K Monitor",
                Description = "27-inch IPS 4K UHD monitor, 144Hz refresh rate, 1ms response time, HDR400.",
                Price = 399.99m,
                Stock = 40,
                CreationDate = DateTime.UtcNow
            },
            new Product
            {
                Name = "Laptop Stand",
                Description = "Adjustable aluminum laptop stand, compatible with 10–17\" laptops, foldable design.",
                Price = 34.99m,
                Stock = 120,
                CreationDate = DateTime.UtcNow
            },
            new Product
            {
                Name = "Webcam HD 1080p",
                Description = "Full HD 1080p webcam with built-in noise-cancelling microphone and auto light correction.",
                Price = 59.99m,
                Stock = 90,
                CreationDate = DateTime.UtcNow
            }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedOrdersAsync(AppDbContext db)
    {
        if (await db.Orders.AnyAsync()) return;

        var customers = await db.Customers.ToListAsync();
        var products = await db.Products.ToListAsync();

        if (!customers.Any() || !products.Any()) return;

        var mouse = products.First(p => p.Name == "Wireless Mouse");
        var keyboard = products.First(p => p.Name == "Mechanical Keyboard");
        var hub = products.First(p => p.Name == "USB-C Hub");
        var monitor = products.First(p => p.Name == "27\" 4K Monitor");
        var stand = products.First(p => p.Name == "Laptop Stand");
        var webcam = products.First(p => p.Name == "Webcam HD 1080p");

        var john = customers.First(c => c.FullName == "John Doe");
        var jane = customers.First(c => c.FullName == "Jane Smith");
        var bob = customers.First(c => c.FullName == "Bob Johnson");

        AddOrder(db, john, OrderStatus.Delivered, "ORD-20250101001", DateTime.UtcNow.AddDays(-30), "Please gift-wrap.", (mouse, 2), (hub, 1));
        AddOrder(db, john, OrderStatus.Processing, "ORD-20250115002", DateTime.UtcNow.AddDays(-10), null, (keyboard, 1), (stand, 1));
        AddOrder(db, jane, OrderStatus.Pending, "ORD-20250120003", DateTime.UtcNow.AddDays(-5), "Leave at door.", (monitor, 1), (webcam, 1), (hub, 2));
        AddOrder(db, bob, OrderStatus.Shipped, "ORD-20250118004", DateTime.UtcNow.AddDays(-7), null, (webcam, 1), (mouse, 1));
        AddOrder(db, jane, OrderStatus.Cancelled, "ORD-20250110005", DateTime.UtcNow.AddDays(-20), "Changed my mind.", (keyboard, 2));

        await db.SaveChangesAsync();
    }

    private static void AddOrder(
        AppDbContext db,
        Customer customer,
        OrderStatus status,
        string orderNumber,
        DateTime creationDate,
        string? notes,
        params (Product product, int qty)[] lines)
    {
        var items = lines.Select(l => new OrderItem
        {
            ProductId = l.product.Id,
            Quantity = l.qty,
            UnitPrice = l.product.Price,
            CreationDate = creationDate
        }).ToList();

        db.Orders.Add(new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customer.Id,
            Status = status,
            Notes = notes,
            TotalAmount = items.Sum(i => i.UnitPrice * i.Quantity),
            CreationDate = creationDate,
            OrderItems = items
        });
    }
}
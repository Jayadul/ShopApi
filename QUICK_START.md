# Shop API — Quick Start

## Prerequisites
- .NET 8 SDK → https://dotnet.microsoft.com/download/dotnet/8
- SQL Server (local or Express) with login: `sa` / `abc-1234`
  - OR update the connection string in `Shop.Api.Host/appsettings.json`

---

## Step 1 — Add Migration

```bash
cd Shop.Api.Host
dotnet ef migrations add InitialCreate --project ../Shop.Infrastructure
```

## Step 2 — Update Database

```bash
dotnet ef database update --project ../Shop.Infrastructure
```

> **Note:** Alternatively, skip steps 1 & 2 entirely — the app auto-migrates and seeds on first run.

## Step 3 — Run

```bash
dotnet run --project Shop.Api.Host
```

Open Swagger: **https://localhost:5001/swagger**

---

## Seeded Accounts

| Role     | Email                  | Password      |
|----------|------------------------|---------------|
| Admin    | admin@shop.com         | Admin123!     |
| Customer | john@customer.com      | Customer123!  |
| Customer | jane@customer.com      | Customer123!  |

## Seeded Data
- **6 Products** — Wireless Mouse, Mechanical Keyboard, USB-C Hub, 27" 4K Monitor, Laptop Stand, Webcam HD 1080p
- **3 Customers** — John Doe, Jane Smith, Bob Johnson
- **5 Orders** in various statuses (Pending, Processing, Shipped, Delivered, Cancelled)

---

## Role Permissions

| Endpoint              | Customer | Admin |
|-----------------------|----------|-------|
| GET /api/products     | ✅       | ✅    |
| POST/PUT/DELETE /api/products | ❌ | ✅ |
| GET/POST/PUT/DELETE /api/customers | ❌ | ✅ |
| POST /api/orders      | ✅       | ✅    |
| GET /api/orders       | ✅       | ✅    |

---

## Example: Place an Order

**1. Login** `POST /api/auth/login`
```json
{ "email": "john@customer.com", "password": "Customer123!" }
```

**2. Copy the token** from the response, click **Authorize** in Swagger, paste `Bearer <token>`

**3. Create an order** `POST /api/orders`
```json
{
  "customerId": 1,
  "notes": "Please deliver before 5pm",
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 3, "quantity": 1 }
  ]
}
```

---

## Connection String

Edit `Shop.Api.Host/appsettings.json` if your SQL Server credentials differ:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=ShopDB;User Id=sa;Password=abc-1234;TrustServerCertificate=True;"
}
```

# ShopAPI

Enterprise-grade REST API built with **.NET 8**, Clean Architecture, CQRS, EF Core, and JWT authentication.

**Live:** https://shop.jayadulshuvo.com/swagger (coming soon)

---

## Quick Start

### Option A — Docker (zero setup, recommended)

```bash
docker compose up --build
```

- API → http://localhost:8080/swagger  
- SQL Server → localhost:1433 (sa / abc-1234)

Database is auto-migrated and seeded on first run. No manual steps needed.

### Option B — Run locally

**1. Restore & migrate**

```bash
cd Shop.Api.Host
dotnet ef migrations add InitialCreate --project ../Shop.Infrastructure --startup-project .
dotnet ef database update --project ../Shop.Infrastructure --startup-project .
```

**2. Run**

```bash
dotnet run --project Shop.Api.Host
```

Swagger → https://localhost:5001/swagger

Connection string and JWT are pre-configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ShopDB;User Id=sa;Password=abc-1234;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Key": "ShopApi_SuperSecretKey_MustBeAtLeast64CharactersLongForHmacSha512Algorithm!!",
    "Issuer": "ShopApi",
    "Audience": "ShopApiClients",
    "ExpiryMinutes": "60"
  }
}
```

---

## Seeded Accounts

| Role     | Email                | Password     |
|----------|----------------------|--------------|
| Admin    | admin@shop.com       | Admin123!    |
| Customer | john@customer.com    | Customer123! |
| Customer | jane@customer.com    | Customer123! |

**Seeded products:** Wireless Mouse · Mechanical Keyboard · USB-C Hub · 27" 4K Monitor · Laptop Stand · Webcam HD 1080p

**Seeded orders:** 5 orders across all statuses — Pending, Processing, Shipped, Delivered, Cancelled

---

## API Testing — Postman Collection

A ready-to-use Postman collection is included in the repo:

📂 **[ShopAPI.postman_collection.json](./ShopAPI.postman_collection.json)**

**Import:** Postman → `File → Import` → select the file.

The collection includes:
- **Auto token saving** — Login requests save the JWT to `{{token}}` automatically, used by all protected requests
- **38 requests** across Auth, Products, Customers, Orders
- **Negative tests** — 401, 403, 400 validation error cases
- **End-to-End flow folder** — 7-step walkthrough: login as Admin → create product → create customer → login as Customer → browse products → place multi-item order → verify order

---

## Role Permissions

| Endpoint                       | Customer | Admin |
|--------------------------------|----------|-------|
| POST /api/auth/register        | ✅ public | ✅ public |
| POST /api/auth/login           | ✅ public | ✅ public |
| GET  /api/products             | ✅        | ✅     |
| POST /api/products             | ❌        | ✅     |
| PUT  /api/products/{id}        | ❌        | ✅     |
| DELETE /api/products/{id}      | ❌        | ✅     |
| GET  /api/customers            | ❌        | ✅     |
| POST /api/customers            | ❌        | ✅     |
| PUT  /api/customers/{id}       | ❌        | ✅     |
| DELETE /api/customers/{id}     | ❌        | ✅     |
| GET  /api/orders               | ✅        | ✅     |
| POST /api/orders               | ✅        | ✅     |

---

## Solution Structure

```
Shop.sln
├── Shop.Domain            → Entities, Enums (zero dependencies)
├── Shop.Application       → CQRS Handlers, DTOs, Interfaces, Validators, MappingProfile
├── Shop.Infrastructure    → EF Core, Repositories, TokenService, DbSeeder
├── Shop.Api.Host          → Controllers, Middleware, Program.cs, appsettings.json
└── Shop.Tests             → Unit Tests (Moq) + Integration Tests (WebApplicationFactory)
```

**Dependency direction:** `Api.Host → Application + Infrastructure → Domain`. Application never references Infrastructure.

---

## Architecture

### Clean Architecture + CQRS

Every feature lives in its own folder:

```
Shop.Application/Features/{Feature}/
    Commands/Create/   → Command · Handler · Validator
    Commands/Update/   → Command · Handler · Validator
    Commands/Delete/   → Command · Handler
    Queries/GetAll/    → Query · Handler
    Queries/GetById/   → Query · Handler
    DTOs/              → XxxDto
```

Handlers return `ApiResponse<T>` wrapped in `IActionResult`. Controllers are thin — they set `CreatedBy` from the JWT claim and forward to MediatR.

### FluentValidation Pipeline

`ValidationBehavior<TRequest, TResponse>` runs all validators before the handler. Failures throw `ValidationException`, caught by `ExceptionHandlingMiddleware` and returned as `400 Bad Request` with a list of error messages.

### Soft Delete

No record is ever physically deleted. `IsArchived = true` hides it via a global EF Core `HasQueryFilter` on every entity configuration.

---

## Database

### Indexes

| Table       | Index |
|-------------|-------|
| Customers   | `IX_Customers_Email` (unique), `IX_Customers_IsArchived` |
| Products    | `IX_Products_Name`, `IX_Products_IsArchived` |
| Orders      | `IX_Orders_Status_CreationDate` (composite), `IX_Orders_CustomerId` |
| OrderItems  | `IX_OrderItems_OrderId`, `IX_OrderItems_ProductId` |
| Users       | `IX_Users_Email` (unique) |

### N+1 Prevention

All read queries use `AsNoTracking()` + `.Include().ThenInclude()`:

```csharp
_db.Orders
   .AsNoTracking()
   .Include(x => x.Customer)
   .Include(x => x.OrderItems)
       .ThenInclude(oi => oi.Product)
```

EF Core translates this to a single SQL JOIN. Lazy loading is disabled.

### Order Creation — Batch Product Loading

`CreateOrderHandler` loads all requested products in one query to avoid N+1:

```csharp
var products = await _productRepository.GetByIdsAsync(requestedProductIds, ct);
```

Validates stock, snapshots `UnitPrice`, deducts stock, calculates `TotalAmount` — all before saving.

---

## Security

### JWT + Password Hashing

- **Algorithm:** HMACSHA512 with a unique random salt per user
- **Critical:** Salt is read from `hmac.Key` **before** calling `ComputeHash` — HMACSHA512 can mutate its key buffer during hashing; reading it after gives a different value, breaking all logins
- **Token claims:** `NameIdentifier`, `Name`, `Email`, `Role` (stored as string `"Admin"` / `"Customer"` so `[Authorize(Roles = "Admin")]` works directly)

### GDPR

| Principle | Implementation |
|-----------|---------------|
| Data Minimisation | DTOs only — entities are never serialised directly |
| Right to Erasure | Soft delete (`IsArchived = true`) — hidden from queries, audit trail preserved |
| Integrity | Email uniqueness at DB level (unique index) + application level (`EmailExistsAsync`) |
| Storage Limitation | Pattern: `BackgroundService` purges `IsArchived = true AND UpdatedDate < NOW() - retention` |

### Input Validation

- FluentValidation on every Command/Query — rejects malformed input before hitting the DB
- EF Core parameterised queries throughout — no SQL injection surface
- JSON-only API — no XSS surface

---

## Testing

```bash
dotnet test
```

### Unit Tests

| Class | Cases |
|---|---|
| `CreateCustomerHandlerTests` | Happy path, duplicate email, repository exception |
| `CreateOrderHandlerTests` | Correct total calculation, customer not found, product not found, insufficient stock |

Moq for repository mocking · FluentAssertions for readable assertions.

### Integration Tests

`ShopWebApplicationFactory` swaps SQL Server for EF InMemory and seeds test users/data. Tests exercise the full HTTP pipeline.

| Test | Verifies |
|---|---|
| `Register_ValidUser_ReturnsOk` | Registration endpoint |
| `Login_ValidAdminCredentials_ReturnsTokenDto` | Login + seeded admin account works |
| `GetCustomers_WithoutToken_Returns401` | Auth middleware |
| `CreateCustomer_WithValidAdminToken_ReturnsOk` | Admin role access |
| `CreateCustomer_WithCustomerToken_Returns403` | Role restriction enforcement |

---

## Docker

### Local development

```bash
docker compose up --build    # build image and start API + SQL Server
docker compose down          # stop
docker compose down -v       # stop and wipe the database volume
```

### Why the NuGet.Config is needed

Running `docker compose up --build` on a Windows dev machine can fail with:

```
NuGet.Packaging.Core.PackagingException: Unable to find fallback package folder
'C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages'
```

The `NuGet.Config` at the solution root explicitly clears `fallbackPackageFolders`, and the Dockerfile uses `dotnet restore --force` to discard stale `project.assets.json` files. Both files must stay in the repo.

### Security

- Multi-stage build — SDK image is not shipped in the final image
- Runs as non-root `appuser`
- Only port 8080 exposed
- All secrets passed as environment variables at runtime, never baked into the image

---

## CI/CD

Push to `main` triggers `.github/workflows/deploy.yml`:

1. **Build & Test** — `dotnet restore` → `dotnet build` → `dotnet test` (failures block deploy)
2. **Docker Build** — builds `shopapi:main` image on the runner
3. **Deploy** — saves image to `.tar`, copies to VPS via `scp`, loads and restarts container on port 5050
4. **Nginx + SSL** — configures reverse proxy for `shop.jayadulshuvo.com` on first deploy, issues Let's Encrypt certificate

### Required GitHub Secrets

Go to **Settings → Secrets and variables → Actions** and add:

| Secret | Description |
|--------|-------------|
| `SERVER_IP` | VPS IP address |
| `DEPLOY_USER` | SSH username (e.g. `root`) |
| `DEPLOY_KEY` | SSH private key (contents of `~/.ssh/id_rsa`) |
| `EMAIL` | Email for Let's Encrypt TLS certificate |

---

## Part B — DB Optimisation Notes

**Execution plans (SSMS):** Run with *Include Actual Execution Plan* and verify:
- **Index Seek** not Scan on filtered columns
- **Nested Loop Join** is efficient for small sets; Hash Join for large ones
- **Key Lookup** on a plan means you need covering columns added to that index

---

## Part C — Audit Logging Guide

Add a MediatR pipeline behavior for production:

```csharp
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    // Log: who (userId from JWT claims), what (TRequest type name),
    //      when (UTC), result (success/fail), old/new values
    // Sink: Serilog structured log → MSSQL AuditLogs table or Azure Monitor
}
```

`AuditLogs` table: `Id · UserId · Action · EntityType · EntityId · Timestamp · IpAddress · OldValues · NewValues`

### Data Retention

- `BackgroundService` (or Hangfire) runs nightly
- Permanently deletes: `IsArchived = true AND UpdatedDate < NOW() - RetentionPeriod`
- Retention period configurable per entity in `appsettings.json`

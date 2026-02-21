# Shop API — Senior .NET Technical Test

## Solution Structure

```
Shop.sln
├── Shop.Domain            → Entities, Enums (no dependencies)
├── Shop.Application       → CQRS Handlers, DTOs, Interfaces, Validators
├── Shop.Infrastructure    → EF Core, Repositories, JWT TokenService
├── Shop.Api.Host          → Controllers, Middleware, Program.cs
└── Shop.Tests             → Unit Tests + Integration Tests
```

## Getting Started

### 1. Configure Secrets (never commit connection strings)

```bash
cd Shop.Api.Host
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=ShopDB;User Id=sa;Password=abc-1234;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:Key" "SuperSecretKey_MinimumOf32CharactersRequired!"
dotnet user-secrets set "JwtSettings:Issuer" "ShopApi"
dotnet user-secrets set "JwtSettings:Audience" "ShopApiClients"
dotnet user-secrets set "JwtSettings:ExpiryMinutes" "60"
```

### 2. Apply Migrations

```bash
cd Shop.Api.Host
dotnet ef migrations add InitialCreate --project ../Shop.Infrastructure --startup-project .
dotnet ef database update --project ../Shop.Infrastructure --startup-project .
```

### 3. Run

```bash
dotnet run --project Shop.Api.Host
# Swagger: https://localhost:5001/swagger
```

---

## Part B — Database & MSSQL Optimization

### Schema Design

**Customers table**
- `IX_Customers_Email` — unique index; fast lookup + prevents duplicates
- `IX_Customers_IsArchived` — filtered index for active-only queries

**Orders table**
- `IX_Orders_Status_CreationDate` — composite index; covers date range + status filtering (matches WHERE clause column order)
- `IX_Orders_CustomerId` — foreign key index; prevents full scan when loading customer's orders

### N+1 Prevention
All repository queries use `.Include()` eagerly:
```csharp
_db.Orders.AsNoTracking().Include(x => x.Customer)
```
EF Core translates this to a single SQL JOIN — no lazy loading enabled, which would cause N+1 loops.

### EF Core Performance Practices
- `AsNoTracking()` on all read queries — no change tracking overhead
- Pagination via `.Skip().Take()` with COUNT done in single pass
- `HasQueryFilter` at entity configuration level for soft-delete filtering (globally applied)
- No `Select *` — projection via DTO mapping

### Execution Plans (Guidance)
Run queries in SSMS with "Include Actual Execution Plan" to verify:
- Index Seek (not Scan) on filtered columns
- Nested Loop Join is efficient for small result sets; Hash Join for large ones
- Watch for Key Lookup — add covering columns to index if needed

---

## Part C — Security & Compliance

### JWT Authentication
- HMACSHA512 password hashing with random salt per user
- JWT signed with HS512; validated on every request via `JwtBearerDefaults.AuthenticationScheme`
- All secrets stored in User Secrets (dev) or environment variables (prod/docker)

### GDPR Principles Applied
| Principle | Implementation |
|-----------|---------------|
| Data Minimisation | DTOs expose only required fields; no raw entity serialisation |
| Right to Erasure | Soft delete (`IsArchived = true`) preserves audit trail while hiding from active queries |
| Storage Limitation | Data retention policy should purge archived records after legal retention period (see below) |
| Integrity | Email uniqueness enforced at DB + application level |

### Input Validation & Security
- FluentValidation on every Command/Query — rejects malformed input before hitting DB
- EF Core parameterised queries throughout — zero raw SQL concatenation, eliminates SQL injection
- No raw HTML output — JSON-only API eliminates XSS surface
- `[Authorize]` on all resource endpoints; only `/api/auth/*` is public

### Audit Logging (Implementation Guide)
For production, add a MediatR pipeline behavior that logs every command/query:
```csharp
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    // Log: who (userId from Claims), what (TRequest type), when (UTC timestamp), result (success/fail)
    // Write to: Serilog structured logs → MSSQL AuditLogs table or Azure Monitor
}
```
AuditLog table columns: `Id, UserId, Action, EntityType, EntityId, Timestamp, IpAddress, OldValues, NewValues`

### Data Retention Policy
- Scheduled background job (e.g. Hangfire or .NET BackgroundService) runs nightly
- Permanently deletes records where `IsArchived = true AND UpdatedDate < NOW() - RetentionPeriod`
- Retention period configured per entity type in `appsettings.json`
- Before deletion, export anonymised aggregate data if needed for analytics

---

## Part D — Testing & DevOps

### Unit Tests
- `CreateCustomerHandlerTests` — validates happy path, duplicate email, repository exception
- `CreateOrderHandlerTests` — validates order creation, customer-not-found guard
- Moq used to mock `ICustomerRepository`, `IOrderRepository`
- FluentAssertions for readable, expressive assertions

### Integration Tests
- `ShopWebApplicationFactory` replaces SQL Server with EF InMemory database
- Tests full HTTP pipeline: routing → middleware → handler → repository → response
- Covers: register, login, unauthorized access, create customer with token

### CI/CD Pipeline (GitHub Actions Example)
```yaml
# .github/workflows/ci.yml
name: CI
on: [push, pull_request]
jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal
      - run: docker build -t shop-api .   # verify image builds
```
- Unit + integration tests run on every PR and push
- Test failures block merge
- For CD: push Docker image to registry, deploy to K8s or App Service

### Docker

```bash
# Build and run locally
docker compose up --build

# API available at http://localhost:8080/swagger
```

**Security hardening in Dockerfile:**
- Multi-stage build (SDK image not in final image)
- Runs as non-root `appuser`
- Only port 8080 exposed
- Secrets injected via environment variables at runtime, never baked into image

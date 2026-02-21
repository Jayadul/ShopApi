# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet.Config so restore uses only nuget.org (no Windows fallback paths)
COPY NuGet.Config ./

# Copy project files for layer-cached restore
COPY Shop.sln ./
COPY Shop.Domain/Shop.Domain.csproj                 Shop.Domain/
COPY Shop.Application/Shop.Application.csproj       Shop.Application/
COPY Shop.Infrastructure/Shop.Infrastructure.csproj Shop.Infrastructure/
COPY Shop.Api.Host/Shop.Api.Host.csproj             Shop.Api.Host/
COPY Shop.Tests/Shop.Tests.csproj                   Shop.Tests/

# Restore (obj/ is excluded via .dockerignore — no stale Windows assets.json present)
RUN dotnet restore Shop.sln

# Copy remaining source
COPY . .

# Publish
RUN dotnet publish Shop.Api.Host/Shop.Api.Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Shop.Api.Host.dll"]

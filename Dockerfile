# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore (layer caching)
COPY Shop.Domain/Shop.Domain.csproj Shop.Domain/
COPY Shop.Application/Shop.Application.csproj Shop.Application/
COPY Shop.Infrastructure/Shop.Infrastructure.csproj Shop.Infrastructure/
COPY Shop.Api.Host/Shop.Api.Host.csproj Shop.Api.Host/
RUN dotnet restore Shop.Api.Host/Shop.Api.Host.csproj

# Copy source and build
COPY . .
RUN dotnet publish Shop.Api.Host/Shop.Api.Host.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Run as non-root for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Shop.Api.Host.dll"]

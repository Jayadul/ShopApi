using Microsoft.EntityFrameworkCore;
using Serilog;
using Shop.Api.Host.Extensions;
using Shop.Api.Host.Middleware;
using Shop.Application.Extensions;
using Shop.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/shop-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Service Registration
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddAuthorization();

var app = builder.Build();

// Database Migration & Seeding
// Only runs on Relational DBs (SQL Server). Skipped for In-Memory tests.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Shop.Infrastructure.Persistence.AppDbContext>();
    if (db.Database.IsRelational())
    {
        await Shop.Infrastructure.Persistence.DbSeeder.SeedAsync(db);
    }
}

// Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
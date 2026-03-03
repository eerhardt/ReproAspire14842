using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddAzureNpgsqlDbContext<MyDbContext>("db");

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

var api = app.MapGroup("/api");
api.MapGet("dbtest", async (MyDbContext dbContext) =>
{
    await dbContext.Database.ExecuteSqlRawAsync(
        "CREATE TABLE IF NOT EXISTS test_runs (id SERIAL PRIMARY KEY, created_at TIMESTAMPTZ DEFAULT NOW())");
    await dbContext.Database.ExecuteSqlRawAsync(
        "INSERT INTO test_runs DEFAULT VALUES");
    var count = await dbContext.Database.SqlQueryRaw<int>(
        "SELECT COUNT(*)::int AS \"Value\" FROM test_runs").SingleAsync();
    return Results.Ok(new { success = true, count });
});

api.MapGet("weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
}

using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TaxManager.Api.Endpoints;
using TaxManager.Api.Middleware;
using TaxManager.Application;
using TaxManager.Infrastructure;
using TaxManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.MapTaxRecordEndpoints();
app.MapTaxRateEndpoints();

// Applies pending EF Core migrations on startup so `docker compose up` / `dotnet run` produce a
// ready-to-use database with no manual migration step.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaxManagerDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

public partial class Program;

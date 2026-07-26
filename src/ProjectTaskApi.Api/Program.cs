using Microsoft.EntityFrameworkCore;
using ProjectTaskApi.Api.Errors;
using ProjectTaskApi.Application;
using ProjectTaskApi.Infrastructure;
using ProjectTaskApi.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Applied here so `docker compose up` is the only command a reviewer needs. Scoped to
    // Development deliberately: migrating automatically on production startup races between
    // instances and removes the chance to review a schema change before it is applied.
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

app.MapControllers();

app.Run();

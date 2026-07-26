using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectTaskApi.Application.Projects.Interfaces;
using ProjectTaskApi.Application.Tasks.Interfaces;
using ProjectTaskApi.Infrastructure.Persistence;
using ProjectTaskApi.Infrastructure.Persistence.Repositories;

namespace ProjectTaskApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' was not found in configuration.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}

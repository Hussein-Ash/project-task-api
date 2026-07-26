using Microsoft.Extensions.DependencyInjection;
using ProjectTaskApi.Application.Projects;
using ProjectTaskApi.Application.Tasks;

namespace ProjectTaskApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();

        return services;
    }
}

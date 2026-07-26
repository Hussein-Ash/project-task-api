using Microsoft.Extensions.DependencyInjection;
using ProjectTaskApi.Application.Projects.Interfaces;
using ProjectTaskApi.Application.Projects.Services;
using ProjectTaskApi.Application.Tasks.Interfaces;
using ProjectTaskApi.Application.Tasks.Services;

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

using ProjectTaskApi.Application.Projects.Dtos;
using ProjectTaskApi.Application.Tasks.Utilities;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Application.Projects.Utilities;

public static class ProjectMappings
{
    public static ProjectResponse ToResponse(this Project project) =>
        new(project.Id, project.Name, project.CreatedAt);

    public static ProjectDetailResponse ToDetailResponse(this Project project) =>
        new(project.Id, project.Name, project.CreatedAt, project.Tasks.ToResponses());
}

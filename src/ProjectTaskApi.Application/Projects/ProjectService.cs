using ProjectTaskApi.Application.Common;
using ProjectTaskApi.Application.Projects.Dtos;
using ProjectTaskApi.Domain.Entities;
using ProjectTaskApi.Domain.Exceptions;

namespace ProjectTaskApi.Application.Projects;

public sealed class ProjectService(IProjectRepository projectRepository) : IProjectService
{
    public async Task<PagedResult<ProjectResponse>> GetPagedAsync(
        PageRequest request,
        CancellationToken cancellationToken)
    {
        var (projects, totalCount) = await projectRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = projects.Select(project => project.ToResponse()).ToList();

        return new PagedResult<ProjectResponse>(items, request.Page, request.PageSize, totalCount);
    }

    public async Task<ProjectDetailResponse> GetByIdAsync(
        Guid id,
        bool? completed,
        CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdWithTasksAsync(id, completed, cancellationToken)
            ?? throw new ProjectNotFoundException(id);

        return project.ToDetailResponse();
    }

    public async Task<ProjectResponse> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        // The factory owns trimming and the whitespace guard, so the service stays free of validation.
        var project = Project.Create(request.Name);

        await projectRepository.AddAsync(project, cancellationToken);

        return project.ToResponse();
    }
}

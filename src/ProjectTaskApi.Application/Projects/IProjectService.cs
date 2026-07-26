using ProjectTaskApi.Application.Common;
using ProjectTaskApi.Application.Projects.Dtos;

namespace ProjectTaskApi.Application.Projects;

public interface IProjectService
{
    Task<PagedResult<ProjectResponse>> GetPagedAsync(
        PageRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the project and its tasks, optionally filtered by completion state.
    /// </summary>
    /// <exception cref="Domain.Exceptions.ProjectNotFoundException">No project has that ID.</exception>
    Task<ProjectDetailResponse> GetByIdAsync(
        Guid id,
        bool? completed,
        CancellationToken cancellationToken);

    Task<ProjectResponse> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken);
}

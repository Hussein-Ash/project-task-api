using ProjectTaskApi.Application.Tasks.Dtos;

namespace ProjectTaskApi.Application.Tasks.Interfaces;

public interface ITaskService
{
    /// <exception cref="Domain.Exceptions.ProjectNotFoundException">No project has that ID.</exception>
    Task<TaskResponse> CreateAsync(
        Guid projectId,
        CreateTaskRequest request,
        CancellationToken cancellationToken);

    /// <exception cref="Domain.Exceptions.TaskNotFoundException">No task has that ID.</exception>
    Task<TaskResponse> UpdateAsync(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken);

    /// <exception cref="Domain.Exceptions.TaskNotFoundException">No task has that ID.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

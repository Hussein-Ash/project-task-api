using ProjectTaskApi.Application.Projects.Interfaces;
using ProjectTaskApi.Application.Tasks.Dtos;
using ProjectTaskApi.Application.Tasks.Interfaces;
using ProjectTaskApi.Application.Tasks.Utilities;
using ProjectTaskApi.Domain.Entities;
using ProjectTaskApi.Domain.Exceptions;

namespace ProjectTaskApi.Application.Tasks.Services;

public sealed class TaskService(
    ITaskRepository taskRepository,
    IProjectRepository projectRepository) : ITaskService
{
    public async Task<TaskResponse> CreateAsync(
        Guid projectId,
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        // Checked explicitly so a missing project surfaces as 404 rather than a foreign key violation.
        if (!await projectRepository.ExistsAsync(projectId, cancellationToken))
        {
            throw new ProjectNotFoundException(projectId);
        }

        var task = TaskItem.Create(projectId, request.Title);

        await taskRepository.AddAsync(task, cancellationToken);

        return task.ToResponse();
    }

    public async Task<TaskResponse> UpdateAsync(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new TaskNotFoundException(id);

        // Completed is non-null here: [Required] on a nullable bool rejects an absent value.
        // The entity keeps CompletedAt in step with the flag.
        task.Update(request.Title, request.Completed!.Value);

        await taskRepository.UpdateAsync(task, cancellationToken);

        return task.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new TaskNotFoundException(id);

        await taskRepository.DeleteAsync(task, cancellationToken);
    }
}

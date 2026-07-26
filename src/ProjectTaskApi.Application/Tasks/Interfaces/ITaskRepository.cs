using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Application.Tasks.Interfaces;

public interface ITaskRepository
{
    /// <summary>Returns the task, or <c>null</c> if no task has that ID.</summary>
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken);

    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken);
}

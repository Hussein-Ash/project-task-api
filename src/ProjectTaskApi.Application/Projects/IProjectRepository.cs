using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Application.Projects;

public interface IProjectRepository
{
    /// <summary>
    /// Returns one page of projects ordered by creation date descending, along with the
    /// total row count. The ordering is deliberate: paging an unordered query returns
    /// arbitrary overlapping rows between pages.
    /// </summary>
    Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns a project with its tasks, or <c>null</c> if no project has that ID.
    /// When <paramref name="completed"/> has a value the tasks are filtered to match it.
    /// </summary>
    Task<Project?> GetByIdWithTasksAsync(
        Guid id,
        bool? completed,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Project project, CancellationToken cancellationToken);
}

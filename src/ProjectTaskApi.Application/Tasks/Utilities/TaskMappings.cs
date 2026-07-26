using ProjectTaskApi.Application.Tasks.Dtos;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Application.Tasks.Utilities;

/// <summary>
/// Explicit mapping in place of a convention-based mapper: a reviewer can read exactly
/// what crosses the boundary, and a renamed property breaks the build rather than the
/// response payload.
/// </summary>
public static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem task) =>
        new(task.Id, task.ProjectId, task.Title, task.Completed, task.CompletedAt, task.CreatedAt);

    public static IReadOnlyList<TaskResponse> ToResponses(this IEnumerable<TaskItem> tasks) =>
        [.. tasks.Select(task => task.ToResponse())];
}

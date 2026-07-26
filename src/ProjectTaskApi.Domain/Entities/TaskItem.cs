using ProjectTaskApi.Domain.Exceptions;

namespace ProjectTaskApi.Domain.Entities;

/// <summary>
/// A unit of work belonging to a <see cref="Project"/>. Named <c>TaskItem</c> rather than
/// <c>Task</c> to avoid colliding with <see cref="System.Threading.Tasks.Task"/> throughout
/// an async codebase; the table is still <c>tasks</c> and the route is still <c>/tasks</c>.
/// </summary>
public sealed class TaskItem
{
    public const int TitleMaxLength = 200;

    private TaskItem(Guid id, Guid projectId, string title, DateTimeOffset createdAt)
    {
        Id = id;
        ProjectId = projectId;
        Title = title;
        Completed = false;
        CompletedAt = null;
        CreatedAt = createdAt;
    }

    /// <summary>Required by EF Core to materialise the entity; not for application use.</summary>
    private TaskItem()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Title { get; private set; } = null!;

    public bool Completed { get; private set; }

    /// <summary>
    /// When the task was completed, or <c>null</c> while it is outstanding. Kept in step with
    /// <see cref="Completed"/> by <see cref="Update"/>, so the two can never disagree.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Project Project { get; private set; } = null!;

    /// <summary>
    /// Creates a task against a project. New tasks always start incomplete, so
    /// <c>completed</c> is deliberately not a parameter.
    /// </summary>
    public static TaskItem Create(Guid projectId, string title) =>
        new(Guid.CreateVersion7(), projectId, Validate(title), DateTimeOffset.UtcNow);

    /// <summary>
    /// Applies a full replacement, matching PUT semantics. <see cref="ProjectId"/> is
    /// intentionally absent: tasks cannot move between projects.
    /// </summary>
    public void Update(string title, bool completed)
    {
        Title = Validate(title);

        if (completed && !Completed)
        {
            CompletedAt = DateTimeOffset.UtcNow;
        }
        else if (!completed)
        {
            // Reopening a task discards the old completion time rather than keeping a
            // stale one that no longer describes the task's state.
            CompletedAt = null;
        }

        // Re-saving an already-completed task leaves CompletedAt alone: it records when the
        // task first became complete, not when it was last written.
        Completed = completed;
    }

    private static string Validate(string title)
    {
        var trimmed = title?.Trim() ?? string.Empty;

        return trimmed.Length == 0
            ? throw new DomainValidationException("Task title must not be empty or whitespace.")
            : trimmed;
    }
}

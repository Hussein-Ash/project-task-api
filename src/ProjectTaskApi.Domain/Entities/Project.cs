using ProjectTaskApi.Domain.Exceptions;

namespace ProjectTaskApi.Domain.Entities;

public sealed class Project
{
    public const int NameMaxLength = 200;

    private readonly List<TaskItem> _tasks = [];

    private Project(Guid id, string name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }

    /// <summary>Required by EF Core to materialise the entity; not for application use.</summary>
    private Project()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<TaskItem> Tasks => _tasks;

    /// <summary>
    /// The only way to bring a project into existence. Trims the name, rejects
    /// whitespace-only input, and stamps the identity and creation time so those
    /// invariants cannot be bypassed by a caller.
    /// </summary>
    public static Project Create(string name)
    {
        // UUID v7 is time-ordered, so inserts append to the index rather than fragmenting it.
        return new Project(Guid.CreateVersion7(), Validate(name), DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Applies a full replacement. <see cref="CreatedAt"/> and the task collection are
    /// deliberately untouched: renaming a project does not re-date it or disturb its tasks.
    /// </summary>
    public void Update(string name) => Name = Validate(name);

    private static string Validate(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;

        return trimmed.Length == 0
            ? throw new DomainValidationException("Project name must not be empty or whitespace.")
            : trimmed;
    }
}

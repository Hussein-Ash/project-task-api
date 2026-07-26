using System.Reflection;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.UnitTests;

/// <summary>
/// Builds entities in states the public API cannot reach directly.
/// <para>
/// A project's tasks are populated by EF Core through a private backing field, deliberately:
/// application code should not be able to graft a task collection onto a project. Tests still
/// need a project that already has tasks, so the field is set reflectively here rather than by
/// widening the domain's surface for the benefit of the test suite.
/// </para>
/// </summary>
internal static class TestEntities
{
    private static readonly FieldInfo TasksField =
        typeof(Project).GetField("_tasks", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException(
            "Project._tasks was not found. If the backing field was renamed, update this helper.");

    public static Project ProjectWith(string name, params TaskItem[] tasks)
    {
        var project = Project.Create(name);

        var backingList = (List<TaskItem>)TasksField.GetValue(project)!;
        backingList.AddRange(tasks);

        return project;
    }

    public static TaskItem CompletedTask(Guid projectId, string title)
    {
        var task = TaskItem.Create(projectId, title);
        task.Update(title, completed: true);

        return task;
    }
}

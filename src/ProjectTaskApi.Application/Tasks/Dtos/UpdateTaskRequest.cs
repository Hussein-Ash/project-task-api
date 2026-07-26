using System.ComponentModel.DataAnnotations;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Application.Tasks.Dtos;

/// <summary>
/// A full replacement, so both fields are required. <c>ProjectId</c> is absent because
/// tasks cannot move between projects.
/// </summary>
public sealed record UpdateTaskRequest
{
    [Required]
    [StringLength(TaskItem.TitleMaxLength, MinimumLength = 1)]
    public string Title { get; init; } = null!;

    /// <summary>
    /// Nullable so that omitting it fails validation. A non-nullable bool would silently
    /// default to false, which would make PUT a partial update in disguise.
    /// </summary>
    [Required]
    public bool? Completed { get; init; }
}

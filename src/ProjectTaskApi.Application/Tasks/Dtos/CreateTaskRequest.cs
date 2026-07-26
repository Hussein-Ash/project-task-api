using System.ComponentModel.DataAnnotations;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Application.Tasks.Dtos;

/// <summary>
/// <c>Completed</c> is deliberately absent: new tasks always start incomplete.
/// </summary>
public sealed record CreateTaskRequest
{
    [Required]
    [StringLength(TaskItem.TitleMaxLength, MinimumLength = 1)]
    public string Title { get; init; } = null!;
}

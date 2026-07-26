using System.ComponentModel.DataAnnotations;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Application.Projects.Dtos;

/// <summary>
/// A full replacement. Only the name is mutable: <c>createdAt</c> is a fact about the
/// project's history, and its tasks are managed through their own endpoints.
/// </summary>
public sealed record UpdateProjectRequest
{
    [Required]
    [StringLength(Project.NameMaxLength, MinimumLength = 1)]
    public string Name { get; init; } = null!;
}

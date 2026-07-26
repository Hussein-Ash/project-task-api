using System.ComponentModel.DataAnnotations;
using ProjectTaskApi.Domain.Entities;

namespace ProjectTaskApi.Application.Projects.Dtos;

public sealed record CreateProjectRequest
{
    /// <summary>
    /// Whitespace-only values pass <c>[Required]</c>, so the domain factory trims and
    /// rejects them; both paths surface as a 400.
    /// </summary>
    [Required]
    [StringLength(Project.NameMaxLength, MinimumLength = 1)]
    public string Name { get; init; } = null!;
}

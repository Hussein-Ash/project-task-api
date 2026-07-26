using System.ComponentModel.DataAnnotations;

namespace ProjectTaskApi.Application.Common;

/// <summary>
/// Paging query parameters. The ranges are enforced by <c>[ApiController]</c> before the
/// action body runs, so an out-of-range page never reaches a service.
/// </summary>
public sealed record PageRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    /// <summary>Capped at 100 to bound response size.</summary>
    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

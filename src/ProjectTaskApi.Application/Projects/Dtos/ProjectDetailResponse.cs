using ProjectTaskApi.Application.Tasks.Dtos;

namespace ProjectTaskApi.Application.Projects.Dtos;

public sealed record ProjectDetailResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    IReadOnlyList<TaskResponse> Tasks);

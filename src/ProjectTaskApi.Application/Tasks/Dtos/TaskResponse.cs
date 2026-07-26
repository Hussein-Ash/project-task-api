namespace ProjectTaskApi.Application.Tasks.Dtos;

public sealed record TaskResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    bool Completed,
    DateTimeOffset CreatedAt);

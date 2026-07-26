namespace ProjectTaskApi.Application.Projects.Dtos;

public sealed record ProjectResponse(Guid Id, string Name, DateTimeOffset CreatedAt);

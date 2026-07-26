namespace ProjectTaskApi.Domain.Exceptions;

/// <summary>Raised when a project is requested by an ID that does not exist. Maps to 404.</summary>
public sealed class ProjectNotFoundException(Guid id)
    : DomainException($"Project with ID '{id}' was not found.");

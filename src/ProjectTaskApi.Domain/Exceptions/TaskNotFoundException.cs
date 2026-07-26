namespace ProjectTaskApi.Domain.Exceptions;

/// <summary>Raised when a task is requested by an ID that does not exist. Maps to 404.</summary>
public sealed class TaskNotFoundException(Guid id)
    : DomainException($"Task with ID '{id}' was not found.");

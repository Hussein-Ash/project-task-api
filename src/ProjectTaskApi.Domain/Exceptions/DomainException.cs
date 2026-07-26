namespace ProjectTaskApi.Domain.Exceptions;

/// <summary>
/// Base type for every error the domain raises deliberately. The API's exception
/// handler maps each concrete subclass to an HTTP status code, which is what keeps
/// try/catch out of the controllers.
/// </summary>
public abstract class DomainException(string message) : Exception(message);

namespace ProjectTaskApi.Domain.Exceptions;

/// <summary>
/// Raised when a value reaching the domain violates an invariant that DataAnnotations
/// cannot express, most notably whitespace-only text that <c>[Required]</c> accepts.
/// Maps to 400.
/// </summary>
public sealed class DomainValidationException(string message) : DomainException(message);

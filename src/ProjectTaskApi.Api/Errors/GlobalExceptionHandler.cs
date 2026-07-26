using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjectTaskApi.Domain.Exceptions;

namespace ProjectTaskApi.Api.Errors;

/// <summary>
/// Translates domain exceptions into RFC 9457 Problem Details responses. Centralising this
/// is what keeps try/catch out of the controllers: an action either returns its result or
/// throws, and every failure leaves the API in the same shape.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string BadRequestType = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
    private const string NotFoundType = "https://tools.ietf.org/html/rfc9110#section-15.5.5";
    private const string ServerErrorType = "https://tools.ietf.org/html/rfc9110#section-15.6.1";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, type, detail) = exception switch
        {
            DomainValidationException => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                BadRequestType,
                exception.Message),

            ProjectNotFoundException or TaskNotFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                NotFoundType,
                exception.Message),

            // Anything unrecognised is a bug. Log it in full, tell the client nothing.
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                ServerErrorType,
                "An unexpected error occurred while processing the request.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Type = type,
                Title = title,
                Status = statusCode,
                Detail = detail,
                Instance = httpContext.Request.Path,
            },
        });
    }
}

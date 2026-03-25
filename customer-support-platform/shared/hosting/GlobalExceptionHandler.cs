using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomerSupportPlatform.Hosting;

/// <summary>
/// Maps unhandled exceptions to RFC 7807-style <see cref="ProblemDetails"/> including correlation id.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IHostEnvironment environment, ILogger<GlobalExceptionHandler> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception");

        var correlationId = httpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString();

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://httpstatuses.com/500",
            Instance = httpContext.Request.Path.Value
        };

        if (!string.IsNullOrEmpty(correlationId))
            problemDetails.Extensions["correlationId"] = correlationId;

        if (_environment.IsDevelopment())
            problemDetails.Detail = exception.ToString();

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
        return true;
    }
}

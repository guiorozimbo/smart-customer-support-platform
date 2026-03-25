using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerSupportPlatform.Hosting;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers correlation-aware exception handling and <see cref="ProblemDetails"/> for APIs.
    /// </summary>
    public static IServiceCollection AddPlatformApiObservability(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Correlation id → structured request logging. Call after <see cref="ExceptionHandlerExtensions.UseExceptionHandler"/> / <see cref="DeveloperExceptionPageExtensions.UseDeveloperExceptionPage"/>.
    /// </summary>
    public static IApplicationBuilder UsePlatformRequestLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}

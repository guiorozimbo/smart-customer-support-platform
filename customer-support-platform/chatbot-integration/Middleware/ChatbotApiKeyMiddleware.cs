using ChatbotIntegration.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChatbotIntegration.Middleware;

/// <summary>
/// Optional shared secret for Blip / HTTP Provider calls. When <see cref="ChatbotSecurityOptions.SharedSecret"/> is empty, validation is skipped (local dev).
/// </summary>
public sealed class ChatbotApiKeyMiddleware
{
    public const string ApiKeyHeaderName = "X-Chatbot-Api-Key";

    private readonly RequestDelegate _next;
    private readonly ChatbotSecurityOptions _options;

    public ChatbotApiKeyMiddleware(RequestDelegate next, IOptions<ChatbotSecurityOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(_options.SharedSecret))
        {
            await _next(context);
            return;
        }

        var provided = context.Request.Headers[ApiKeyHeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(provided) || !string.Equals(provided, _options.SharedSecret, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unauthorized",
                detail = "Valid X-Chatbot-Api-Key header is required.",
                status = 401
            });
            return;
        }

        await _next(context);
    }
}

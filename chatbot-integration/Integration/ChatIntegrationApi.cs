using System.Text.Json.Serialization;

namespace ChatbotIntegration.Integration;

// ——— Contratos HTTP (Minimal API) ———

public sealed record InboundMessageRequest(string? Text);

public sealed record OutboundTextPayload(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("content")] string Content)
{
    public static OutboundTextPayload PlainText(string content) =>
        new("text/plain", content);
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
[JsonSerializable(typeof(InboundMessageRequest))]
[JsonSerializable(typeof(OutboundTextPayload))]
public partial class ChatIntegrationJsonContext : JsonSerializerContext;

// ——— Lógica de resposta (demo / apresentação) ———

public static class ChatIntegrationReply
{
    public static string FromText(string? text)
    {
        var t = text?.Trim() ?? "";
        return t switch
        {
            "" =>
                "Escreva algo para continuar. Palavras-chave de demo: projeto, status.",

            var x when x.Equals("projeto", StringComparison.OrdinalIgnoreCase) =>
                "Plataforma Customer Support: .NET 10, API Gateway (YARP), microserviços de pedidos, clientes e tickets, " +
                "Entity Framework Core, SQL Server e integração HTTP. REST detalhado em /api/chatbot/*; conversa rápida em POST /api/integration/messages.",

            var x when x.Equals("status", StringComparison.OrdinalIgnoreCase) =>
                "Status (demonstração): integração OK | base de dados contactável | serviços simulados estáveis.",

            _ =>
                "Não reconheci o pedido. Experimente projeto ou status."
        };
    }
}

// ——— Registo de endpoints ———

public static class ChatIntegrationEndpoints
{
    public static WebApplication MapChatIntegrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/integration")
            .WithTags("Integração (Minimal API)");

        group.MapPost("/messages", HandleInboundMessage)
            .WithName("PostIntegrationMessage")
            .WithSummary("Recebe texto de um canal externo e devolve resposta em texto.")
            .Produces<OutboundTextPayload>(StatusCodes.Status200OK, "application/json");

        return app;
    }

    private static IResult HandleInboundMessage(
        InboundMessageRequest? body,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var input = body?.Text;
        Console.WriteLine($"[chat-integration] entrada: \"{input ?? "(null)"}\" @ {DateTime.UtcNow:O}");

        var content = ChatIntegrationReply.FromText(input);
        var payload = OutboundTextPayload.PlainText(content);

        return TypedResults.Json(payload, ChatIntegrationJsonContext.Default.OutboundTextPayload);
    }
}

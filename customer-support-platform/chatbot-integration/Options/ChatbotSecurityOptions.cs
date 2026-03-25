namespace ChatbotIntegration.Options;

public sealed class ChatbotSecurityOptions
{
    public const string SectionName = "Chatbot";

    /// <summary>
    /// When set, requests must send matching <c>X-Chatbot-Api-Key</c>. Leave empty for development.
    /// </summary>
    public string? SharedSecret { get; set; }
}

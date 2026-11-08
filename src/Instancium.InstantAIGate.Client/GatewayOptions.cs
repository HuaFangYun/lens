namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Configuration options for InstantAIGate client.
/// </summary>
public sealed record GatewayOptions
{
    /// <summary>
    /// Base URL of the gateway (e.g., "http://localhost:5000").
    /// </summary>
    public string BaseUrl { get; init; } = "http://localhost:5000";

    /// <summary>
    /// Hub path (default: "/hub/gateway").
    /// </summary>
    public string HubPath { get; init; } = "/hub/gateway";

    /// <summary>
    /// API key for authentication (User role).
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Embedding model repository ID.
    /// </summary>
    public string EmbeddingRepoId { get; init; } = "qwen3-embedding-0.6b";

    /// <summary>
    /// LLM model repository ID (empty for primary model).
    /// </summary>
    public string LlmRepoId { get; init; } = string.Empty;

    /// <summary>
    /// Expected embedding dimension for validation.
    /// </summary>
    public int ExpectedEmbeddingDimension { get; init; } = 1024;

    /// <summary>Context retention policy applied to clients created by the factory.</summary>
    public ChatContextPolicy ContextPolicy { get; init; } = ChatContextPolicy.Stateless;

    /// <summary>
    /// Reconnection delays for SignalR connection.
    /// </summary>
    public IReadOnlyList<TimeSpan> ReconnectDelays { get; init; } =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10)
    ];

    /// <summary>
    /// Validates required configuration.
    /// </summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(BaseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(HubPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(ApiKey);

        if (ExpectedEmbeddingDimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ExpectedEmbeddingDimension),
                "Embedding dimension must be positive.");
        }
    }

    /// <summary>
    /// Returns the full hub URL.
    /// </summary>
    public string GetHubUrl() => $"{BaseUrl.TrimEnd('/')}{HubPath}";
}

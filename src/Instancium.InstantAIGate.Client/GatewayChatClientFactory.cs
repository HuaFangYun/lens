using Microsoft.Extensions.Logging;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Default <see cref="IChatClientFactory"/>: composes a new GatewayChatClient with
/// the strategy matching the requested policy. Strategies are cheap per-instance
/// objects, so each created client owns an isolated session lifecycle.
/// </summary>
public sealed class GatewayChatClientFactory : IChatClientFactory
{
    private readonly IGatewayConnection _connection;
    private readonly GatewayOptions _options;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>Initializes the factory with the shared gateway connection plane.</summary>
    public GatewayChatClientFactory(
        IGatewayConnection connection,
        GatewayOptions options,
        ILoggerFactory loggerFactory)
    {
        _connection = connection;
        _options = options;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public IChatClient Create(ChatContextPolicy policy) =>
        new GatewayChatClient(
            _connection,
            _options,
            CreateStrategy(policy),
            _loggerFactory.CreateLogger<GatewayChatClient>());

    private static IChatContextStrategy CreateStrategy(ChatContextPolicy policy) => policy switch
    {
        ChatContextPolicy.Stateless => new StatelessChatContextStrategy(),
        ChatContextPolicy.Conversational => new ConversationalChatContextStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(policy), $"Unsupported context policy: {policy}.")
    };
}

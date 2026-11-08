using System.Threading.Channels;
using InstantAIGate.Core.Dtos.Inference;
using InstantAIGate.Core.Dtos.Session;
using InstantAIGate.Core.Exceptions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Chat client that uses InstantAIGate via SignalR with streaming support.
/// </summary>
public sealed class GatewayChatClient : IChatClient
{
    private readonly IGatewayConnection _connection;
    private readonly GatewayOptions _options;
    private readonly ILogger<GatewayChatClient> _logger;

    /// <summary>
    /// Initializes the client with gateway connection.
    /// </summary>
    public GatewayChatClient(
        IGatewayConnection connection,
        GatewayOptions options,
        ILogger<GatewayChatClient> logger)
    {
        _connection = connection;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> CompleteAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var builder = new System.Text.StringBuilder();

        await StreamAsync(
            prompt,
            token =>
            {
                builder.Append(token);
                return Task.CompletedTask;
            },
            cancellationToken);

        return builder.ToString();
    }

    /// <inheritdoc />
    public async Task StreamAsync(
        string prompt,
        Func<string, Task> onToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        ArgumentNullException.ThrowIfNull(onToken);

        var hub = await _connection.GetConnectionAsync(cancellationToken);
        var sessionId = $"lens-{Guid.NewGuid():N}";
        var channel = Channel.CreateUnbounded<string>();

        // WHY: every terminal server event must complete the channel, otherwise the
        // consumer loop below never exits and the UI hangs in an opaque state (D-1).
        void OnDelta(SessionTokenDelta delta)
        {
            if (delta.SessionId != sessionId)
            {
                return;
            }

            if (delta.IsDone)
            {
                channel.Writer.TryComplete();
            }
            else
            {
                channel.Writer.TryWrite(delta.Content);
            }
        }

        void OnError(string message) =>
            channel.Writer.TryComplete(new InvalidOperationException(message));

        void OnOverflow(ContextOverflowReport report) =>
            channel.Writer.TryComplete(report.ToException());

        Task OnClosed(Exception? error)
        {
            channel.Writer.TryComplete(new InvalidOperationException(
                "Gateway connection closed before the stream completed.", error));
            return Task.CompletedTask;
        }

        var deltaSubscription = hub.On<SessionTokenDelta>("ReceiveTokenDelta", OnDelta);
        var errorSubscription = hub.On<string>("ReceiveError", OnError);
        var overflowSubscription = hub.On<ContextOverflowReport>("ReceiveContextOverflow", OnOverflow);
        hub.Closed += OnClosed;
        try
        {
            _logger.LogDebug("Starting chat session {SessionId}", sessionId);
            await hub.InvokeAsync("JoinSession", sessionId, _options.LlmRepoId, cancellationToken);
            var message = new ChatMessage(
                "user",
                new List<MessageContent> { new TextContent(prompt) });
            await hub.InvokeAsync("SendPromptDelta", sessionId, message, cancellationToken);
            await foreach (var token in channel.Reader.ReadAllAsync(cancellationToken))
            {
                await onToken(token);
            }

            _logger.LogDebug("Chat session {SessionId} completed", sessionId);
        }
        finally
        {
            deltaSubscription.Dispose();
            errorSubscription.Dispose();
            overflowSubscription.Dispose();
            hub.Closed -= OnClosed;
            try
            {
                await hub.InvokeAsync("LeaveSession", sessionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to leave session {SessionId}", sessionId);
            }
        }
    }
}

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
        var completion = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        // Event handlers
        void OnDelta(SessionTokenDelta delta)
        {
            if (delta.SessionId != sessionId)
            {
                return;
            }

            if (delta.IsDone)
            {
                completion.TrySetResult(delta.FinishReason ?? "stop");
            }
            else
            {
                channel.Writer.TryWrite(delta.Content);
            }
        }

        void OnError(string message)
        {
            if (!completion.Task.IsCompleted)
            {
                completion.TrySetException(new InvalidOperationException(message));
            }
        }

        void OnOverflow(ContextOverflowReport report)
        {
            if (!completion.Task.IsCompleted)
            {
                completion.TrySetException(report.ToException());
            }
        }

        // Subscribe to events
        var deltaSubscription = hub.On<SessionTokenDelta>("ReceiveTokenDelta", OnDelta);
        var errorSubscription = hub.On<string>("ReceiveError", OnError);
        var overflowSubscription = hub.On<ContextOverflowReport>("ReceiveContextOverflow", OnOverflow);

        try
        {
            _logger.LogDebug("Starting chat session {SessionId}", sessionId);

            // Join session
            await hub.InvokeAsync(
                "JoinSession",
                sessionId,
                _options.LlmRepoId,
                cancellationToken);

            // Send prompt
            var message = new ChatMessage(
                "user",
                new List<MessageContent> { new TextContent(prompt) });

            await hub.InvokeAsync(
                "SendPromptDelta",
                sessionId,
                message,
                cancellationToken);

            // Stream tokens to callback
            await foreach (var token in channel.Reader.ReadAllAsync(cancellationToken))
            {
                await onToken(token);
            }

            // Wait for completion
            await completion.Task;

            _logger.LogDebug("Chat session {SessionId} completed", sessionId);
        }
        finally
        {
            // Unsubscribe from events
            deltaSubscription.Dispose();
            errorSubscription.Dispose();
            overflowSubscription.Dispose();

            // Leave session
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

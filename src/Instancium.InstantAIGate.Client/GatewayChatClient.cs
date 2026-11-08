using System.Diagnostics;
using System.Threading.Channels;
using InstantAIGate.Core.Dtos.Inference;
using InstantAIGate.Core.Dtos.Session;
using InstantAIGate.Core.Exceptions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Chat client over InstantAIGate (SignalR) whose session lifecycle is delegated to
/// an <see cref="IChatContextStrategy"/>. Transport-only: context retention decisions
/// live in the strategy selected at creation time.
/// </summary>
public sealed class GatewayChatClient : IChatClient, IAsyncDisposable
{
    private readonly IGatewayConnection _connection;
    private readonly GatewayOptions _options;
    private readonly IChatContextStrategy _strategy;
    private readonly ILogger<GatewayChatClient> _logger;
    private IChatSessionScope? _lifetimeScope;

    /// <summary>Initializes the client with connection, options and context strategy.</summary>
    public GatewayChatClient(
        IGatewayConnection connection,
        GatewayOptions options,
        IChatContextStrategy strategy,
        ILogger<GatewayChatClient> logger)
    {
        _connection = connection;
        _options = options;
        _strategy = strategy;
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
        var scope = _strategy.BeginScope();
        _lifetimeScope ??= scope;
        var channel = Channel.CreateUnbounded<string>();

        void OnDelta(SessionTokenDelta delta)
        {
            if (delta.SessionId != scope.SessionId)
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

        Task OnReconnected(string connectionId)
        {
            // WHY: parity with the CLI/Publisher reference clients — an in-flight
            // generation cannot survive a transport swap; fail fast so the caller retries.
            channel.Writer.TryComplete(new InvalidOperationException(
                "Gateway reconnected; in-flight stream aborted. Retry the request."));
            return Task.CompletedTask;
        }

        var deltaSubscription = hub.On<SessionTokenDelta>("ReceiveTokenDelta", OnDelta);
        var errorSubscription = hub.On<string>("ReceiveError", OnError);
        var overflowSubscription = hub.On<ContextOverflowReport>("ReceiveContextOverflow", OnOverflow);
        hub.Closed += OnClosed;
        hub.Reconnected += OnReconnected;
        try
        {
            _logger.LogDebug("Starting chat session {SessionId}", scope.SessionId);
            await scope.PrepareAsync(hub, _options.LlmRepoId, cancellationToken);
            var message = new ChatMessage(
                "user",
                new List<MessageContent> { new TextContent(prompt) });

            // WHY: SendAsync is fire-and-forget by SignalR contract. The hub method
            // SendPromptDelta completes only after generation finishes, so awaiting
            // InvokeAsync here buffers every delta server-side and collapses the
            // stream into a single end-burst (observed defect). Server-side faults
            // still surface via ReceiveError / ReceiveContextOverflow / Closed.
            await hub.SendAsync("SendPromptDelta", scope.SessionId, message, cancellationToken);

            var stopwatch = Stopwatch.StartNew();
            var firstTokenLogged = false;
            await foreach (var token in channel.Reader.ReadAllAsync(cancellationToken))
            {
                if (!firstTokenLogged)
                {
                    firstTokenLogged = true;
                    _logger.LogDebug(
                        "Session {SessionId}: first token after {Ms} ms",
                        scope.SessionId, stopwatch.ElapsedMilliseconds);
                }

                await onToken(token);
            }

            _logger.LogDebug(
                "Chat session {SessionId} completed in {Ms} ms",
                scope.SessionId, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            deltaSubscription.Dispose();
            errorSubscription.Dispose();
            overflowSubscription.Dispose();
            hub.Closed -= OnClosed;
            hub.Reconnected -= OnReconnected;
            try
            {
                await scope.SealAsync(hub, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to seal session {SessionId}", scope.SessionId);
            }
        }
    }

    /// <summary>Releases the lifetime session held by the context strategy (no-op for stateless).</summary>
    public async ValueTask DisposeAsync()
    {
        var scope = _lifetimeScope;
        if (scope is null)
        {
            return;
        }

        try
        {
            var hub = await _connection.GetConnectionAsync(CancellationToken.None);
            await scope.CloseAsync(hub, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to close chat session {SessionId}", scope.SessionId);
        }
    }
}

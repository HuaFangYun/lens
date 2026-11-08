using Microsoft.AspNetCore.SignalR.Client;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Multi-turn policy: one session per client instance; KV-cache is retained between
/// interactions and released softly (LeaveSession) when the client is disposed.
/// </summary>
public sealed class ConversationalChatContextStrategy : IChatContextStrategy
{
    private readonly ConversationalSessionScope _scope = new();

    /// <inheritdoc />
    public IChatSessionScope BeginScope() => _scope;

    private sealed class ConversationalSessionScope : IChatSessionScope
    {
        private int _closed;

        public string SessionId { get; } = $"lens-conv-{Guid.NewGuid():N}";

        public Task PrepareAsync(HubConnection hub, string repoId, CancellationToken cancellationToken) =>
            hub.InvokeAsync("JoinSession", SessionId, repoId, cancellationToken);

        public Task SealAsync(HubConnection hub, CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task CloseAsync(HubConnection hub, CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _closed, 1) == 1)
            {
                return;
            }

            await hub.InvokeAsync("LeaveSession", SessionId, cancellationToken);
        }
    }
}

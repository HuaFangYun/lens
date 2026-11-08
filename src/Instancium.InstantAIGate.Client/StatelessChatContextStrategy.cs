using Microsoft.AspNetCore.SignalR.Client;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Single-shot policy: fresh session per interaction, hard destroy afterwards so no
/// history and no KV-cache survive and the VRAM slot returns to the pool immediately.
/// </summary>
public sealed class StatelessChatContextStrategy : IChatContextStrategy
{
    /// <inheritdoc />
    public IChatSessionScope BeginScope() => new StatelessSessionScope();

    private sealed class StatelessSessionScope : IChatSessionScope
    {
        private int _sealed;

        public string SessionId { get; } = $"lens-once-{Guid.NewGuid():N}";

        public Task PrepareAsync(HubConnection hub, string repoId, CancellationToken cancellationToken) =>
            hub.InvokeAsync("JoinSession", SessionId, repoId, cancellationToken);

        public async Task SealAsync(HubConnection hub, CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _sealed, 1) == 1)
            {
                return;
            }

            // WHY: DestroySession frees the native context slot; LeaveSession would
            // only clear KV-cache but keep a pooled context occupied.
            await hub.InvokeAsync("DestroySession", SessionId, cancellationToken);
        }

        public Task CloseAsync(HubConnection hub, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

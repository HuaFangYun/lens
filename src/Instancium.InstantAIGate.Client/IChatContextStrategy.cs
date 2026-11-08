using Microsoft.AspNetCore.SignalR.Client;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Lifetime handle of one chat session against the gateway hub.
/// WHY: session id policy and teardown semantics (Destroy vs Leave vs retain)
/// belong to the context strategy, not to the transport client.
/// </summary>
public interface IChatSessionScope
{
    /// <summary>Gateway session identifier for all hub calls of this scope.</summary>
    string SessionId { get; }

    /// <summary>Creates or resumes the server-side session (idempotent per SessionInferenceManager).</summary>
    Task PrepareAsync(HubConnection hub, string repoId, CancellationToken cancellationToken);

    /// <summary>Per-interaction teardown; runs in finally on success and on failure.</summary>
    Task SealAsync(HubConnection hub, CancellationToken cancellationToken);

    /// <summary>End-of-chat teardown; called once when the owning client is disposed.</summary>
    Task CloseAsync(HubConnection hub, CancellationToken cancellationToken);
}

/// <summary>
/// Factory of session scopes implementing a concrete context retention policy.
/// Extension point: a checkpointed map-reduce strategy (Publisher pattern with
/// GetSessionTokenCount/RollbackSession) plugs in here without touching the client.
/// </summary>
public interface IChatContextStrategy
{
    /// <summary>Opens a scope: fresh per interaction (stateless) or shared per client (conversational).</summary>
    IChatSessionScope BeginScope();
}

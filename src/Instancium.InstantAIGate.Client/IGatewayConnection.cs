using Microsoft.AspNetCore.SignalR.Client;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Manages a SignalR connection to the InstantAIGate hub.
/// Ensures readiness gating before operations.
/// </summary>
public interface IGatewayConnection : IAsyncDisposable
{
    /// <summary>
    /// Gets the underlying hub connection, connecting if necessary.
    /// Verifies gateway readiness before returning.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connected hub connection.</returns>
    Task<HubConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default);
}

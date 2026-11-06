using InstantAIGate.Core.Dtos.Status;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Manages a SignalR connection to the InstantAIGate hub.
/// Handles reconnection and readiness gating.
/// </summary>
public sealed class GatewayConnection : IGatewayConnection
{
    private readonly GatewayOptions _options;
    private readonly ILogger<GatewayConnection> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private HubConnection? _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes the connection manager with gateway options.
    /// </summary>
    public GatewayConnection(
        GatewayOptions options,
        ILogger<GatewayConnection> logger)
    {
        _options = options;
        _logger = logger;
        _options.Validate();
    }

    /// <inheritdoc />
    public async Task<HubConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_connection?.State == HubConnectionState.Connected)
        {
            return _connection;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                return _connection;
            }

            await ConnectAsync(cancellationToken);
            return _connection!;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Connecting to gateway at {HubUrl}", _options.GetHubUrl());

        _connection = new HubConnectionBuilder()
            .WithUrl(_options.GetHubUrl(), httpOptions =>
            {
                httpOptions.AccessTokenProvider = () =>
                    Task.FromResult<string?>(_options.ApiKey);
            })
            .WithAutomaticReconnect(_options.ReconnectDelays.ToArray())
            .Build();

        _connection.Reconnecting += error =>
        {
            _logger.LogWarning(error, "Gateway connection lost, reconnecting...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            _logger.LogInformation("Gateway connection reconnected: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        _connection.Closed += error =>
        {
            _logger.LogWarning(error, "Gateway connection closed");
            return Task.CompletedTask;
        };

        // Check gateway readiness before starting
        await EnsureGatewayReadyAsync(cancellationToken);

        await _connection.StartAsync(cancellationToken);
        _logger.LogInformation("Connected to gateway successfully");
    }

    private async Task EnsureGatewayReadyAsync(CancellationToken cancellationToken)
    {
        // Create a temporary connection to check status
        var tempConnection = new HubConnectionBuilder()
            .WithUrl(_options.GetHubUrl(), httpOptions =>
            {
                httpOptions.AccessTokenProvider = () =>
                    Task.FromResult<string?>(_options.ApiKey);
            })
            .Build();

        try
        {
            await tempConnection.StartAsync(cancellationToken);

            var status = await tempConnection.InvokeAsync<GatewayStatusDetails>(
                "GetGatewayStatus", cancellationToken);

            if (status.Status != GatewayOperationalStatus.Ready)
            {
                throw new InvalidOperationException(
                    $"Gateway not ready: {status.Status}. Stage: {status.StageDescription}");
            }

            _logger.LogInformation("Gateway is ready");
        }
        finally
        {
            await tempConnection.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }
}

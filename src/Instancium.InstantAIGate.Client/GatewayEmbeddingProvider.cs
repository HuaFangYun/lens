using InstantAIGate.Core.Dtos.Inference;
using Instancium.Vectra.Core.Contracts;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Embedding provider over InstantAIGate (SignalR) implementing the Vectra
/// embedding seam, so it can be passed directly to the engine and
/// VectraTextExtensions.
/// </summary>
public sealed class GatewayEmbeddingProvider : IEmbeddingProvider
{
    private readonly IGatewayConnection _connection;
    private readonly GatewayOptions _options;
    private readonly ILogger<GatewayEmbeddingProvider> _logger;
    private readonly SemaphoreSlim _validationLock = new(1, 1);
    private bool _dimensionValidated;

    /// <summary>
    /// Initializes the provider with gateway connection.
    /// </summary>
    public GatewayEmbeddingProvider(
        IGatewayConnection connection,
        GatewayOptions options,
        ILogger<GatewayEmbeddingProvider> logger)
    {
        _connection = connection;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public string ModelId => _options.EmbeddingRepoId;

    /// <inheritdoc />
    public int Dimension => _options.ExpectedEmbeddingDimension;

    /// <inheritdoc />
    public async ValueTask<ReadOnlyMemory<float>> EmbedAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var hub = await _connection.GetConnectionAsync(cancellationToken).ConfigureAwait(false);
        await ValidateDimensionAsync(hub, cancellationToken).ConfigureAwait(false);

        var response = await hub.InvokeAsync<EmbeddingResponse>(
            "GenerateEmbeddingsAsync",
            ModelId,
            new EmbeddingRequest(text, Normalize: true),
            cancellationToken).ConfigureAwait(false);

        return response.Embedding;
    }

    /// <summary>
    /// Generates embeddings for multiple texts (sequential MVP implementation).
    /// </summary>
    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(texts);

        var results = new List<ReadOnlyMemory<float>>(texts.Count);
        foreach (var text in texts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await EmbedAsync(text, cancellationToken).ConfigureAwait(false));
        }

        return results;
    }

    private async Task ValidateDimensionAsync(
        HubConnection hub,
        CancellationToken cancellationToken)
    {
        if (_dimensionValidated)
        {
            return;
        }

        await _validationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_dimensionValidated)
            {
                return;
            }

            var actual = await hub.InvokeAsync<int>(
                "GetEmbeddingDimensionAsync",
                ModelId,
                cancellationToken).ConfigureAwait(false);

            if (actual != Dimension)
            {
                throw new InvalidOperationException(
                    $"Embedding dimension mismatch: gateway reports {actual} for '{ModelId}', but Lens is configured for {Dimension}.");
            }

            _dimensionValidated = true;
            _logger.LogInformation(
                "Embedding dimension confirmed: {Dimension} for {ModelId}",
                Dimension, ModelId);
        }
        finally
        {
            _validationLock.Release();
        }
    }
}

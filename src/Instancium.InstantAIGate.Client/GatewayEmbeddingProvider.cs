using InstantAIGate.Core.Dtos.Inference;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Embedding provider that uses InstantAIGate via SignalR.
/// </summary>
public sealed class GatewayEmbeddingProvider : IEmbeddingProvider
{
    private readonly IGatewayConnection _connection;
    private readonly GatewayOptions _options;
    private readonly ILogger<GatewayEmbeddingProvider> _logger;
    private int _cachedDimension;

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
    public async Task<int> GetDimensionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cachedDimension > 0)
        {
            return _cachedDimension;
        }

        var hub = await _connection.GetConnectionAsync(cancellationToken);

        var dimension = await hub.InvokeAsync<int>(
            "GetEmbeddingDimensionAsync",
            _options.EmbeddingRepoId,
            cancellationToken);

        if (dimension != _options.ExpectedEmbeddingDimension)
        {
            throw new InvalidOperationException(
                $"Embedding dimension mismatch: expected {_options.ExpectedEmbeddingDimension}, got {dimension}");
        }

        _cachedDimension = dimension;
        _logger.LogInformation(
            "Embedding dimension confirmed: {Dimension} for {RepoId}",
            dimension, _options.EmbeddingRepoId);

        return dimension;
    }

    /// <inheritdoc />
    public async Task<float[]> EmbedAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var hub = await _connection.GetConnectionAsync(cancellationToken);

        var request = new EmbeddingRequest(text, Normalize: true);
        var response = await hub.InvokeAsync<EmbeddingResponse>(
            "GenerateEmbeddingsAsync",
            _options.EmbeddingRepoId,
            request,
            cancellationToken);

        return response.Embedding;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(texts);

        var results = new List<float[]>(texts.Count);

        foreach (var text in texts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var embedding = await EmbedAsync(text, cancellationToken);
            results.Add(embedding);
        }

        return results;
    }
}

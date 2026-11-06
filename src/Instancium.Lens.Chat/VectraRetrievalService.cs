using System.Text.Json;
using Instancium.Lens.Core;
using Instancium.Vectra.Core.Contracts;
using Instancium.Vectra.Core.Models;
using Microsoft.Extensions.Logging;

namespace Instancium.Lens.Chat;

/// <summary>
/// Retrieval over a Vectra collection via the public engine API.
/// SearchTextAsync is used instead of raw-vector search to keep the query-text
/// hook required for future rerankers (Vectra guide, anti-patterns §9).
/// Hits arrive with hydrated payloads (VectraCollection.SearchAsync contract),
/// so no secondary payload read is performed here.
/// </summary>
public sealed class VectraRetrievalService : IRetrievalService
{
    private readonly IVectraEngine _engine;
    private readonly IEmbeddingProvider _embeddings;
    private readonly ILogger<VectraRetrievalService> _logger;

    /// <summary>
    /// Initializes the retrieval service with required dependencies.
    /// </summary>
    public VectraRetrievalService(
        IVectraEngine engine,
        IEmbeddingProvider embeddings,
        ILogger<VectraRetrievalService> logger)
    {
        _engine = engine;
        _embeddings = embeddings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<RetrievedChunk>>> RetrieveAsync(
        string profileId,
        string query,
        int topK,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(topK), "topK must be positive.");
        }

        try
        {
            var collection = await Task.Run(
                () => _engine.GetOrOpenCollection(
                    profileId,
                    new CollectionOptions(
                        _embeddings.Dimension,
                        SimilarityMetric.Cosine,
                        _embeddings.ModelId)),
                cancellationToken).ConfigureAwait(false);

            var hits = await collection.SearchTextAsync(
                _embeddings,
                query,
                new SearchOptions(topK: topK),
                cancellationToken).ConfigureAwait(false);

            var chunks = new List<RetrievedChunk>(hits.Count);
            foreach (var hit in hits)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var payload = ParsePayload(hit.Payload);
                chunks.Add(new RetrievedChunk(
                    payload.Source, payload.ChunkIndex, payload.Text));
            }

            _logger.LogDebug(
                "Retrieved {HitCount} chunks for profile {ProfileId}",
                chunks.Count, profileId);
            return Result<IReadOnlyList<RetrievedChunk>>.Success(chunks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Retrieval cancelled for profile {ProfileId}", profileId);
            return Result<IReadOnlyList<RetrievedChunk>>.Failure(
                new LensError("retrieval.cancelled", "Retrieval was cancelled."));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Stored payload is not a valid chunk payload");
            return Result<IReadOnlyList<RetrievedChunk>>.Failure(
                new LensError("retrieval.payload_invalid", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retrieval failed for profile {ProfileId}", profileId);
            return Result<IReadOnlyList<RetrievedChunk>>.Failure(
                new LensError("retrieval.search_failed", ex.Message));
        }
    }

    private static ChunkPayload ParsePayload(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new JsonException("Stored payload text is missing.");
        }

        var payload = JsonSerializer.Deserialize<ChunkPayload>(payloadJson);
        if (payload is null
            || string.IsNullOrWhiteSpace(payload.Text)
            || string.IsNullOrWhiteSpace(payload.Source))
        {
            throw new JsonException("Chunk payload is missing required fields.");
        }

        return payload;
    }
}

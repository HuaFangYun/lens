using System.Text.Json;
using Instancium.Chunker;
using Instancium.FileParser;
using Instancium.Lens.Core;
using Instancium.Vectra.Core.Contracts;
using Instancium.Vectra.Core.Models;
using Microsoft.Extensions.Logging;

namespace Instancium.Lens.Ingestion;

/// <summary>
/// Orchestrates the indexing pipeline: parse → chunk → embed → store.
/// Built against the real Instancium.Vectra 1.1.1 API.
/// </summary>
public sealed class IngestionService : IIngestionService
{
    private static readonly string[] SupportedExtensions = [".txt", ".md"];

    private readonly IVectraEngine _engine;
    private readonly IEmbeddingProvider _embeddings;
    private readonly IReadOnlyList<IFileParser> _parsers;
    private readonly IChunker _chunker;
    private readonly ILogger<IngestionService> _logger;

    /// <summary>
    /// Initializes the ingestion service with required dependencies.
    /// </summary>
    public IngestionService(
        IVectraEngine engine,
        IEmbeddingProvider embeddings,
        IEnumerable<IFileParser> parsers,
        IChunker chunker,
        ILogger<IngestionService> logger)
    {
        _engine = engine;
        _embeddings = embeddings;
        _parsers = parsers.ToList();
        _chunker = chunker;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<IndexingResult>> IndexFolderAsync(
        string profileId,
        string folderPath,
        IProgress<IndexingProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        if (!Directory.Exists(folderPath))
        {
            return Result<IndexingResult>.Failure(
                new LensError(
                    "ingestion.folder_not_found",
                    $"Folder not found: {folderPath}"));
        }

        try
        {
            _logger.LogInformation(
                "Starting indexing for profile {ProfileId} from {FolderPath}",
                profileId, folderPath);

            // MVP: rebuild the collection from scratch on every run.
            await _engine.DeleteCollectionAsync(profileId, cancellationToken)
                .ConfigureAwait(false);

            // GetOrOpenCollection is synchronous (disk I/O inside Vectra);
            // hop to the thread pool so the MAUI/Blazor sync context stays free.
            var collection = await Task.Run(
                () => _engine.GetOrOpenCollection(
                    profileId,
                    new CollectionOptions(
                        _embeddings.Dimension,
                        SimilarityMetric.Cosine,
                        _embeddings.ModelId)),
                cancellationToken).ConfigureAwait(false);

            // remove: private static readonly string[] SupportedExtensions = [".txt", ".md"];
            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(SupportedDocuments.IsSupported)
                .ToList();

            _logger.LogInformation("Found {FileCount} files to index", files.Count);

            var totalFiles = files.Count;
            var processedFiles = 0;
            var totalChunks = 0;

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var chunksIndexed = await IndexFileAsync(collection, file, cancellationToken)
                    .ConfigureAwait(false);

                totalChunks += chunksIndexed;
                processedFiles++;

                progress?.Report(new IndexingProgress(
                    processedFiles, totalFiles, totalChunks));

                _logger.LogDebug("Indexed file {File}: {Chunks} chunks", file, chunksIndexed);
            }

            _logger.LogInformation(
                "Indexing complete: {Files} files, {Chunks} chunks",
                processedFiles, totalChunks);

            return Result<IndexingResult>.Success(
                new IndexingResult(processedFiles, totalChunks));
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Indexing cancelled");
            return Result<IndexingResult>.Failure(
                new LensError("ingestion.cancelled", "Indexing was cancelled."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Indexing failed");
            return Result<IndexingResult>.Failure(
                new LensError("ingestion.failed", ex.Message));
        }
    }

    private async Task<int> IndexFileAsync(
        IVectraCollection collection,
        string filePath,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(filePath);
        var parser = _parsers.FirstOrDefault(p => p.CanParse(extension));

        if (parser is null)
        {
            _logger.LogWarning("No parser found for {File}, skipping", filePath);
            return 0;
        }

        var document = await parser.ParseAsync(filePath, cancellationToken)
            .ConfigureAwait(false);

        var chunks = await _chunker
            .ChunkAsync(filePath, document.NormalizedText, cancellationToken)
            .ConfigureAwait(false);

        if (chunks.Count == 0)
        {
            return 0;
        }

        var entries = new VectorEntry[chunks.Count];

        for (var i = 0; i < chunks.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunk = chunks[i];
            var vector = await _embeddings.EmbedAsync(chunk.Text, cancellationToken)
                .ConfigureAwait(false);

            var payload = JsonSerializer.Serialize(new ChunkPayload
            {
                Version = ChunkPayload.CurrentVersion,
                Text = chunk.Text,
                Source = chunk.SourcePath,
                ChunkIndex = chunk.Index
            });
            entries[i] = new VectorEntry(vectorId: null, vector, payload);
        }

        // One disk flush per file instead of one per chunk.
        await collection.AddRangeAsync(entries, cancellationToken).ConfigureAwait(false);

        return entries.Length;
    }
}

using System.Diagnostics;
using System.Text.Json;
using Instancium.Chunker;
using Instancium.FileParser;
using Instancium.Lens.Core;
using Instancium.Vectra.Core.Contracts;
using Instancium.Vectra.Core.Models;
using Microsoft.Extensions.Logging;

namespace Instancium.Lens.Ingestion;

/// <summary>
/// Rebuilds a profile collection from every supported document in a folder.
/// </summary>
public sealed class IngestionService : IIngestionService
{
    /// <summary>
    /// Minimum interval between chunk-level progress reports.
    /// </summary>
    /// <remarks>
    /// Embedding is CPU-bound; reporting every chunk would flood the UI
    /// dispatcher queue on large corpora without adding visible precision.
    /// </remarks>
    private const int ReportIntervalMs = 100;

    private readonly IVectraEngine _engine;
    private readonly IEmbeddingProvider _embeddings;
    private readonly IChunker _chunker;
    private readonly IReadOnlyList<IFileParser> _parsers;
    private readonly ILogger<IngestionService> _logger;

    /// <summary>
    /// Creates the service.
    /// </summary>
    /// <param name="engine">Vector storage engine.</param>
    /// <param name="embeddings">Embedding model provider.</param>
    /// <param name="chunker">Text chunking strategy.</param>
    /// <param name="parsers">Document parsers resolved by extension.</param>
    /// <param name="logger">Logger.</param>
    public IngestionService(
        IVectraEngine engine,
        IEmbeddingProvider embeddings,
        IChunker chunker,
        IEnumerable<IFileParser> parsers,
        ILogger<IngestionService> logger)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
        _chunker = chunker ?? throw new ArgumentNullException(nameof(chunker));
        _parsers = (parsers ?? throw new ArgumentNullException(nameof(parsers))).ToList();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                profileId,
                folderPath);

            await _engine.DeleteCollectionAsync(profileId, cancellationToken)
                .ConfigureAwait(false);

            var collection = await Task.Run(
                () => _engine.GetOrOpenCollection(
                    profileId,
                    new CollectionOptions(
                        _embeddings.Dimension,
                        SimilarityMetric.Cosine,
                        _embeddings.ModelId)),
                cancellationToken).ConfigureAwait(false);

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

                progress?.Report(new IndexingProgress(
                    processedFiles,
                    totalFiles,
                    totalChunks,
                    file));

                var chunksIndexed = await IndexFileAsync(
                        collection,
                        file,
                        progress,
                        processedFiles,
                        totalFiles,
                        totalChunks,
                        cancellationToken)
                    .ConfigureAwait(false);

                totalChunks += chunksIndexed;
                processedFiles++;

                progress?.Report(new IndexingProgress(
                    processedFiles,
                    totalFiles,
                    totalChunks));

                _logger.LogDebug("Indexed file {File}: {Chunks} chunks", file, chunksIndexed);
            }

            _logger.LogInformation(
                "Indexing complete: {Files} files, {Chunks} chunks",
                processedFiles,
                totalChunks);

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

    /// <summary>
    /// Parses, chunks and embeds a single file, reporting chunk-level progress.
    /// </summary>
    /// <param name="collection">Target vector collection.</param>
    /// <param name="filePath">Absolute path of the document.</param>
    /// <param name="progress">Optional progress sink; receives throttled reports per embedded chunk.</param>
    /// <param name="processedFiles">Files completed before this one.</param>
    /// <param name="totalFiles">Total files in the current run.</param>
    /// <param name="chunksIndexed">Chunks stored before this file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of chunks stored for this file.</returns>
    private async Task<int> IndexFileAsync(
        IVectraCollection collection,
        string filePath,
        IProgress<IndexingProgress>? progress,
        int processedFiles,
        int totalFiles,
        int chunksIndexed,
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

        progress?.Report(new IndexingProgress(
            processedFiles,
            totalFiles,
            chunksIndexed,
            filePath,
            0,
            chunks.Count));

        var entries = new VectorEntry[chunks.Count];
        var throttle = Stopwatch.StartNew();

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

            var isLastChunk = i == chunks.Count - 1;
            if (isLastChunk || throttle.ElapsedMilliseconds >= ReportIntervalMs)
            {
                throttle.Restart();
                progress?.Report(new IndexingProgress(
                    processedFiles,
                    totalFiles,
                    chunksIndexed + i + 1,
                    filePath,
                    i + 1,
                    chunks.Count));
            }
        }

        await collection.AddRangeAsync(entries, cancellationToken).ConfigureAwait(false);
        return entries.Length;
    }
}

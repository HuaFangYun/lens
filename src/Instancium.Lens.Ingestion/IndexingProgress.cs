namespace Instancium.Lens.Ingestion;

/// <summary>
/// Immutable snapshot of a folder indexing operation.
/// </summary>
/// <remarks>
/// Chunk-level counters let the UI render smooth progress while a single large
/// document is embedded, instead of jumping once per file.
/// </remarks>
public sealed record IndexingProgress
{
    /// <summary>
    /// Creates a progress snapshot.
    /// </summary>
    /// <param name="filesProcessed">Files fully indexed so far.</param>
    /// <param name="totalFiles">Files discovered in the folder.</param>
    /// <param name="chunksIndexed">Chunks stored so far, including the current file.</param>
    /// <param name="currentFile">Path of the file being processed; null between files.</param>
    /// <param name="currentFileChunksProcessed">Chunks embedded in the current file.</param>
    /// <param name="currentFileChunksTotal">Chunks the current file was split into; 0 until chunking completes.</param>
    /// <exception cref="ArgumentOutOfRangeException">A counter is negative or exceeds its total.</exception>
    /// <exception cref="ArgumentException">Chunk counters are set without a current file.</exception>
    public IndexingProgress(
        int filesProcessed,
        int totalFiles,
        int chunksIndexed,
        string? currentFile = null,
        int currentFileChunksProcessed = 0,
        int currentFileChunksTotal = 0)
    {
        if (filesProcessed < 0 || filesProcessed > totalFiles)
        {
            throw new ArgumentOutOfRangeException(nameof(filesProcessed));
        }

        if (totalFiles < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalFiles));
        }

        if (chunksIndexed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunksIndexed));
        }

        if (currentFileChunksTotal < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentFileChunksTotal));
        }

        if (currentFileChunksProcessed < 0 || currentFileChunksProcessed > currentFileChunksTotal)
        {
            throw new ArgumentOutOfRangeException(nameof(currentFileChunksProcessed));
        }

        if (currentFileChunksTotal > 0 && currentFile is null)
        {
            throw new ArgumentException(
                "Chunk counters require the current file path.",
                nameof(currentFile));
        }

        FilesProcessed = filesProcessed;
        TotalFiles = totalFiles;
        ChunksIndexed = chunksIndexed;
        CurrentFile = currentFile;
        CurrentFileChunksProcessed = currentFileChunksProcessed;
        CurrentFileChunksTotal = currentFileChunksTotal;
    }

    /// <summary>Files fully indexed so far.</summary>
    public int FilesProcessed { get; }

    /// <summary>Files discovered in the folder.</summary>
    public int TotalFiles { get; }

    /// <summary>Chunks stored so far, including the current file.</summary>
    public int ChunksIndexed { get; }

    /// <summary>Path of the file being processed; null between files.</summary>
    public string? CurrentFile { get; }

    /// <summary>Chunks embedded in the current file.</summary>
    public int CurrentFileChunksProcessed { get; }

    /// <summary>Chunks the current file was split into; 0 until chunking completes.</summary>
    public int CurrentFileChunksTotal { get; }

    /// <summary>
    /// Overall completion percentage (0-100).
    /// </summary>
    /// <remarks>
    /// Includes the fraction of the current file so the bar moves during
    /// embedding of a single large document.
    /// </remarks>
    public int Percentage
    {
        get
        {
            if (TotalFiles <= 0)
            {
                return 0;
            }

            var fileFraction = CurrentFileChunksTotal > 0
                ? (double)CurrentFileChunksProcessed / CurrentFileChunksTotal
                : 0d;
            var completed = FilesProcessed + fileFraction;
            var percentage = (int)Math.Round(completed * 100d / TotalFiles);
            return Math.Clamp(percentage, 0, 100);
        }
    }
}

namespace Instancium.Lens.Ingestion;

/// <summary>
/// Progress information for indexing operation.
/// </summary>
public sealed record IndexingProgress
{
    /// <summary>
    /// Initializes progress with current state.
    /// </summary>
    public IndexingProgress(int filesProcessed, int totalFiles, int chunksIndexed)
    {
        if (filesProcessed < 0 || filesProcessed > totalFiles)
        {
            throw new ArgumentOutOfRangeException(nameof(filesProcessed));
        }

        if (chunksIndexed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunksIndexed));
        }

        FilesProcessed = filesProcessed;
        TotalFiles = totalFiles;
        ChunksIndexed = chunksIndexed;
    }

    /// <summary>
    /// Number of files processed so far.
    /// </summary>
    public int FilesProcessed { get; }

    /// <summary>
    /// Total number of files to process.
    /// </summary>
    public int TotalFiles { get; }

    /// <summary>
    /// Total number of chunks indexed so far.
    /// </summary>
    public int ChunksIndexed { get; }

    /// <summary>
    /// Completion percentage (0-100).
    /// </summary>
    public int Percentage => TotalFiles > 0
        ? (FilesProcessed * 100) / TotalFiles
        : 0;
}

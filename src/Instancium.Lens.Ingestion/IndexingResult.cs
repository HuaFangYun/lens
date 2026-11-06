namespace Instancium.Lens.Ingestion;

/// <summary>
/// Result of an indexing operation.
/// </summary>
public sealed record IndexingResult
{
    /// <summary>
    /// Initializes indexing result with statistics.
    /// </summary>
    public IndexingResult(int filesProcessed, int chunksIndexed)
    {
        if (filesProcessed < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(filesProcessed), "Files processed cannot be negative.");
        }

        if (chunksIndexed < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunksIndexed), "Chunks indexed cannot be negative.");
        }

        FilesProcessed = filesProcessed;
        ChunksIndexed = chunksIndexed;
    }

    /// <summary>
    /// Number of files successfully processed.
    /// </summary>
    public int FilesProcessed { get; }

    /// <summary>
    /// Total number of chunks indexed.
    /// </summary>
    public int ChunksIndexed { get; }
}

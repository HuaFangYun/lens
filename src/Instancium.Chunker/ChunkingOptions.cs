namespace Instancium.Chunker;

/// <summary>
/// Configuration options for text chunking.
/// </summary>
public sealed record ChunkingOptions
{
    /// <summary>
    /// Default chunk size in characters.
    /// </summary>
    public const int DefaultChunkSize = 512;

    /// <summary>
    /// Default overlap size in characters.
    /// </summary>
    public const int DefaultOverlap = 50;

    /// <summary>
    /// Maximum number of characters per chunk.
    /// </summary>
    public int ChunkSize { get; init; } = DefaultChunkSize;

    /// <summary>
    /// Number of overlapping characters between consecutive chunks.
    /// </summary>
    public int Overlap { get; init; } = DefaultOverlap;

    /// <summary>
    /// Validates that options are within acceptable ranges.
    /// </summary>
    public void Validate()
    {
        if (ChunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ChunkSize), "Chunk size must be positive.");
        }

        if (Overlap < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Overlap), "Overlap cannot be negative.");
        }

        if (Overlap >= ChunkSize)
        {
            throw new ArgumentException(
                "Overlap must be less than chunk size.");
        }
    }
}

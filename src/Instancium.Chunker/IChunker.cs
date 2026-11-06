namespace Instancium.Chunker;

/// <summary>
/// Splits text into chunks with optional overlap.
/// Implementations must be stateless and thread-safe.
/// </summary>
public interface IChunker
{
    /// <summary>
    /// Splits the given text into chunks.
    /// </summary>
    /// <param name="sourcePath">Path to the source document (for tracking).</param>
    /// <param name="text">Text to split.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of chunks.</returns>
    Task<IReadOnlyList<Chunk>> ChunkAsync(
        string sourcePath,
        string text,
        CancellationToken cancellationToken = default);
}

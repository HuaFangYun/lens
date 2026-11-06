namespace Instancium.Lens.Chat;

/// <summary>
/// A chunk retrieved from the vector index and hydrated from its stored payload.
/// Storage-format details never leak past the retrieval seam.
/// </summary>
public sealed record RetrievedChunk
{
    /// <summary>
    /// Initializes a retrieved chunk with required fields.
    /// </summary>
    public RetrievedChunk(string source, int chunkIndex, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (chunkIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunkIndex), "Chunk index cannot be negative.");
        }

        Source = source;
        ChunkIndex = chunkIndex;
        Text = text;
    }

    /// <summary>Path to the source document.</summary>
    public string Source { get; }

    /// <summary>Zero-based index of the chunk within the source document.</summary>
    public int ChunkIndex { get; }

    /// <summary>Chunk text content.</summary>
    public string Text { get; }
}

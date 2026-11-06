namespace Instancium.Chunker;

/// <summary>
/// Represents a chunk of text from a source document.
/// </summary>
public sealed record Chunk
{
    /// <summary>
    /// Initializes a chunk with required fields.
    /// </summary>
    public Chunk(string sourcePath, int index, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index), "Chunk index cannot be negative.");
        }

        SourcePath = sourcePath;
        Index = index;
        Text = text;
    }

    /// <summary>
    /// Path to the source document.
    /// </summary>
    public string SourcePath { get; }

    /// <summary>
    /// Zero-based index of this chunk within the source document.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Text content of the chunk.
    /// </summary>
    public string Text { get; }
}

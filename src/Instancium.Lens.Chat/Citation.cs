namespace Instancium.Lens.Chat;

/// <summary>
/// A citation attached to a chat answer.
/// <see cref="Number"/> matches the [n] marker injected into the prompt and the answer.
/// </summary>
public sealed record Citation
{
    /// <summary>
    /// Initializes a citation with required fields.
    /// </summary>
    public Citation(int number, string source, int chunkIndex, string text)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(number), "Citation number must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (chunkIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunkIndex), "Chunk index cannot be negative.");
        }

        Number = number;
        Source = source;
        ChunkIndex = chunkIndex;
        Text = text;
    }

    /// <summary>One-based citation number matching the [n] marker.</summary>
    public int Number { get; }

    /// <summary>Path to the cited source document.</summary>
    public string Source { get; }

    /// <summary>Zero-based index of the cited chunk within the source document.</summary>
    public int ChunkIndex { get; }

    /// <summary>Cited chunk text; must match the stored payload.</summary>
    public string Text { get; }
}

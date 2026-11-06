using System.Text;

namespace Instancium.Chunker;

/// <summary>
/// Splits text into fixed-size chunks with overlap.
/// Attempts to break on sentence boundaries when possible.
/// </summary>
public sealed class TextChunker : IChunker
{
    private readonly ChunkingOptions _options;

    /// <summary>
    /// Initializes a chunker with the specified options.
    /// </summary>
    public TextChunker(ChunkingOptions? options = null)
    {
        _options = options ?? new ChunkingOptions();
        _options.Validate();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Chunk>> ChunkAsync(
        string sourcePath,
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var chunks = new List<Chunk>();

        if (text.Length <= _options.ChunkSize)
        {
            chunks.Add(new Chunk(sourcePath, 0, text));
            return Task.FromResult<IReadOnlyList<Chunk>>(chunks);
        }

        var position = 0;
        var chunkIndex = 0;

        while (position < text.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunkEnd = Math.Min(
                position + _options.ChunkSize, text.Length);

            // Try to break at a sentence boundary if not at the end
            if (chunkEnd < text.Length)
            {
                chunkEnd = FindSentenceBoundary(text, position, chunkEnd);
            }

            var chunkText = text.Substring(
                position, chunkEnd - position).Trim();

            if (!string.IsNullOrEmpty(chunkText))
            {
                chunks.Add(new Chunk(sourcePath, chunkIndex++, chunkText));
            }

            // Move forward by chunk size minus overlap
            var nextPosition = chunkEnd - _options.Overlap;

            // Ensure we make progress
            if (nextPosition <= position)
            {
                nextPosition = chunkEnd;
            }

            position = nextPosition;
        }

        return Task.FromResult<IReadOnlyList<Chunk>>(chunks);
    }

    private static int FindSentenceBoundary(
        string text, int start, int end)
    {
        // Look for sentence-ending punctuation in the last 20% of the chunk
        var searchStart = Math.Max(
            start, end - (end - start) / 5);

        for (var i = end - 1; i >= searchStart; i--)
        {
            var c = text[i];
            if (c == '.' || c == '!' || c == '?' || c == '\n')
            {
                return i + 1;
            }
        }

        // No sentence boundary found, use the original end
        return end;
    }
}

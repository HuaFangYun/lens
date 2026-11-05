namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Provides embedding generation capabilities.
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Gets the embedding dimension for the configured model.
    /// Validates against expected dimension.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Embedding dimension.</returns>
    Task<int> GetDimensionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an embedding for the given text.
    /// </summary>
    /// <param name="text">Text to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Normalized embedding vector.</returns>
    Task<float[]> EmbedAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for multiple texts.
    /// </summary>
    /// <param name="texts">Texts to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of normalized embedding vectors.</returns>
    Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);
}

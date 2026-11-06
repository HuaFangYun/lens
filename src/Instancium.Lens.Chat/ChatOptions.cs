namespace Instancium.Lens.Chat;

/// <summary>
/// Configuration options for RAG chat orchestration.
/// </summary>
public sealed record ChatOptions
{
    /// <summary>
    /// Default number of context chunks retrieved per question.
    /// Matches the MVP contract (top-5 semantic search).
    /// </summary>
    public const int DefaultTopK = 5;

    /// <summary>
    /// Number of context chunks retrieved per question.
    /// </summary>
    public int TopK { get; init; } = DefaultTopK;

    /// <summary>
    /// Validates that options are within acceptable ranges.
    /// </summary>
    public void Validate()
    {
        if (TopK <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TopK), "TopK must be positive.");
        }
    }
}

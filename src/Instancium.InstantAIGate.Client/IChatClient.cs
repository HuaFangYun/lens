namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Provides LLM chat completion capabilities.
/// </summary>
public interface IChatClient
{
    /// <summary>
    /// Generates a completion for the given prompt.
    /// </summary>
    /// <param name="prompt">User prompt with context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated completion text.</returns>
    Task<string> CompleteAsync(
        string prompt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams completion tokens for the given prompt.
    /// </summary>
    /// <param name="prompt">User prompt with context.</param>
    /// <param name="onToken">Callback for each token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StreamAsync(
        string prompt,
        Func<string, Task> onToken,
        CancellationToken cancellationToken = default);
}

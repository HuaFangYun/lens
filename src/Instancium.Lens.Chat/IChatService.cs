using Instancium.Lens.Core;

namespace Instancium.Lens.Chat;

/// <summary>
/// Orchestrates the RAG question pipeline: retrieve → prompt → complete → cite.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Answers a question grounded in the indexed documents of the given profile.
    /// Failures travel as Result, never as exceptions.
    /// </summary>
    /// <param name="profileId">User profile identifier (collection name).</param>
    /// <param name="question">User question.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result<ChatAnswer>> AskAsync(
        string profileId,
        string question,
        CancellationToken cancellationToken = default);
}

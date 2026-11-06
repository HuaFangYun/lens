using Instancium.Lens.Core;

namespace Instancium.Lens.Chat;

/// <summary>
/// Retrieves relevant chunks from the vector index for a profile.
/// Hides the Vectra read path (collection open, search, payload hydration)
/// so chat orchestration stays storage-agnostic.
/// </summary>
public interface IRetrievalService
{
    /// <summary>
    /// Returns the top chunks most relevant to the query, in relevance order.
    /// </summary>
    /// <param name="profileId">User profile identifier (collection name).</param>
    /// <param name="query">Retrieval query text.</param>
    /// <param name="topK">Maximum number of chunks to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result<IReadOnlyList<RetrievedChunk>>> RetrieveAsync(
        string profileId,
        string query,
        int topK,
        CancellationToken cancellationToken = default);
}

using Instancium.Lens.Core;

namespace Instancium.Lens.Ingestion;

/// <summary>
/// Orchestrates the indexing pipeline: parse → chunk → embed → store.
/// </summary>
public interface IIngestionService
{
    /// <summary>
    /// Indexes all supported files in the given folder.
    /// </summary>
    /// <param name="profileId">User profile identifier (used as collection name).</param>
    /// <param name="folderPath">Path to folder to index.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Indexing result with statistics.</returns>
    Task<Result<IndexingResult>> IndexFolderAsync(
        string profileId,
        string folderPath,
        IProgress<IndexingProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

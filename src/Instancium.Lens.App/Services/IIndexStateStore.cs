using System.Threading;
using System.Threading.Tasks;
using Instancium.Lens.Ingestion;

namespace Instancium.Lens.App.Services;

/// <summary>
/// Persists the last successful indexing summary per profile.
/// WHY: index state is UI telemetry; keeping it out of IngestionService preserves the
/// library contract while giving the chat page real post-restart facts.
/// </summary>
public interface IIndexStateStore
{
    /// <summary>Stores the summary of a completed indexing run.</summary>
    Task RecordAsync(string profileId, IndexingResult result, CancellationToken cancellationToken = default);

    /// <summary>Reads the persisted summary, if the profile was ever indexed.</summary>
    bool TryGetStats(string profileId, out ProfileIndexStats? stats);
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Instancium.Lens.Ingestion;

namespace Instancium.Lens.App.Services;

/// <summary>
/// JSON-file backed <see cref="IIndexStateStore"/> over <see cref="AtomicJsonFile{T}"/>.
/// </summary>
public sealed class IndexStateStore : IIndexStateStore
{
    private readonly AtomicJsonFile<Dictionary<string, ProfileIndexStats>> _file;

    /// <summary>Initializes the store over the per-user AppData state file.</summary>
    public IndexStateStore()
    {
        _file = new AtomicJsonFile<Dictionary<string, ProfileIndexStats>>(
            Path.Combine(FileSystem.AppDataDirectory, "lens-index-stats.json"),
            () => new Dictionary<string, ProfileIndexStats>(StringComparer.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public Task RecordAsync(string profileId, IndexingResult result, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        ArgumentNullException.ThrowIfNull(result);
        var stats = new ProfileIndexStats
        {
            FilesProcessed = result.FilesProcessed,
            ChunksIndexed = result.ChunksIndexed,
            UpdatedAtUtc = DateTime.UtcNow
        };
        return _file.UpdateAsync(
            map =>
            {
                var next = new Dictionary<string, ProfileIndexStats>(map, StringComparer.OrdinalIgnoreCase)
                {
                    [profileId] = stats
                };
                return next;
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public bool TryGetStats(string profileId, out ProfileIndexStats? stats)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        return _file.Read().TryGetValue(profileId, out stats);
    }
}

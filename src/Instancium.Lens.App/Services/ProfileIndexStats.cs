using System;

namespace Instancium.Lens.App.Services;

/// <summary>
/// Persisted indexing summary of one profile.
/// WHY: the chat page must explain what the index contains after a restart without
/// re-scanning the source folder or opening Vectra internals.
/// </summary>
public sealed record ProfileIndexStats
{
    /// <summary>Number of documents indexed in the last successful run.</summary>
    public int FilesProcessed { get; init; }

    /// <summary>Number of chunks stored in the last successful run.</summary>
    public int ChunksIndexed { get; init; }

    /// <summary>UTC timestamp of the last successful run.</summary>
    public DateTime UpdatedAtUtc { get; init; }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace Instancium.Lens.Ingestion;

/// <summary>
/// Single source of truth for document formats accepted by the indexing pipeline.
/// WHY: the UI preview count and the ingestion filter must never drift apart;
/// extensions live here instead of being duplicated per consumer.
/// </summary>
public static class SupportedDocuments
{
    /// <summary>Lower-cased extensions (with dot) accepted by the indexer.</summary>
    public static readonly IReadOnlyList<string> Extensions = [".txt", ".md"];

    /// <summary>True when the file path has a supported extension.</summary>
    public static bool IsSupported(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        foreach (var candidate in Extensions)
        {
            if (candidate == extension)
            {
                return true;
            }
        }
        return false;
    }
}

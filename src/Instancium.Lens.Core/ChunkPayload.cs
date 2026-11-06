using System.Text.Json.Serialization;

namespace Instancium.Lens.Core;

/// <summary>
/// Versioned chunk payload stored alongside each vector (schema v1).
/// Single source of truth shared by the indexing writer and the chat reader.
/// Evolution rule: fields are additive only; bump Version on breaking intent,
/// readers must tolerate unknown fields.
/// </summary>
public sealed record ChunkPayload
{
    /// <summary>Schema version produced by writers.</summary>
    public const int CurrentVersion = 1;

    /// <summary>Payload schema version (wire name: "v").</summary>
    [JsonPropertyName("v")]
    public int Version { get; init; }

    /// <summary>Chunk text content.</summary>
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    /// <summary>Path of the source document.</summary>
    [JsonPropertyName("source")]
    public string Source { get; init; } = string.Empty;

    /// <summary>Zero-based chunk index within the source document.</summary>
    [JsonPropertyName("chunkIndex")]
    public int ChunkIndex { get; init; }
}

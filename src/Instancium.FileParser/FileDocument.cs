namespace Instancium.FileParser;

/// <summary>
/// Represents a parsed document with normalized text and metadata.
/// </summary>
public sealed record FileDocument
{
    /// <summary>
    /// Initializes a document with required metadata.
    /// </summary>
    public FileDocument(
        string sourcePath,
        string normalizedText,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedText);

        SourcePath = sourcePath;
        NormalizedText = normalizedText;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Absolute or relative path to the source file.
    /// </summary>
    public string SourcePath { get; }

    /// <summary>
    /// Normalized text content with consistent line endings and whitespace.
    /// </summary>
    public string NormalizedText { get; }

    /// <summary>
    /// Optional metadata (author, date, title, etc.).
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}

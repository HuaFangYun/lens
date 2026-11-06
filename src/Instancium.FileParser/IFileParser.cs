namespace Instancium.FileParser;

/// <summary>
/// Parses a file into a normalized document.
/// Implementations must be stateless and thread-safe.
/// </summary>
public interface IFileParser
{
    /// <summary>
    /// Checks if this parser supports the given file extension.
    /// </summary>
    /// <param name="fileExtension">Extension including dot (e.g., ".txt").</param>
    bool CanParse(string fileExtension);

    /// <summary>
    /// Parses the file at the given path.
    /// </summary>
    /// <param name="filePath">Absolute or relative path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed document with normalized text and metadata.</returns>
    Task<FileDocument> ParseAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}

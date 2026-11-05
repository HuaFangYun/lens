using System.Text;

namespace Instancium.FileParser;

/// <summary>
/// Parses plain text files (.txt) with basic normalization.
/// </summary>
public sealed class TextFileParser : IFileParser
{
    /// <inheritdoc />
    public bool CanParse(string fileExtension)
    {
        return string.Equals(
            fileExtension, ".txt", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<FileDocument> ParseAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Text file not found.", filePath);
        }

        var rawText = await File.ReadAllTextAsync(
            filePath, Encoding.UTF8, cancellationToken);

        var normalizedText = NormalizeText(rawText);

        var metadata = new Dictionary<string, string>
        {
            ["fileType"] = "text",
            ["fileName"] = Path.GetFileName(filePath)
        };

        return new FileDocument(filePath, normalizedText, metadata);
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Normalize line endings to \n
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");

        var lines = normalized.Split('\n');
        var builder = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                // Collapse multiple spaces to single space
                var collapsed = string.Join(
                    " ",
                    trimmed.Split([' '], StringSplitOptions.RemoveEmptyEntries));
                builder.AppendLine(collapsed);
            }
            else
            {
                // Preserve paragraph breaks
                builder.AppendLine();
            }
        }

        return builder.ToString().Trim();
    }
}

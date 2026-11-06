using System.Text;
using System.Text.RegularExpressions;

namespace Instancium.FileParser;

/// <summary>
/// Parses Markdown files (.md) with heading extraction.
/// </summary>
public sealed partial class MarkdownFileParser : IFileParser
{
    /// <inheritdoc />
    public bool CanParse(string fileExtension)
    {
        return string.Equals(
            fileExtension, ".md", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<FileDocument> ParseAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Markdown file not found.", filePath);
        }

        var rawText = await File.ReadAllTextAsync(
            filePath, Encoding.UTF8, cancellationToken);

        var normalizedText = NormalizeMarkdown(rawText);

        var metadata = new Dictionary<string, string>
        {
            ["fileType"] = "markdown",
            ["fileName"] = Path.GetFileName(filePath)
        };

        // Extract title from first H1 heading if present
        var titleMatch = TitleRegex().Match(rawText);
        if (titleMatch.Success)
        {
            metadata["title"] = titleMatch.Groups[1].Value.Trim();
        }

        return new FileDocument(filePath, normalizedText, metadata);
    }

    private static string NormalizeMarkdown(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");

        var lines = normalized.Split('\n');
        var builder = new StringBuilder();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                var collapsed = string.Join(
                    " ",
                    trimmed.Split([' '], StringSplitOptions.RemoveEmptyEntries));
                builder.AppendLine(collapsed);
            }
            else
            {
                builder.AppendLine();
            }
        }

        return builder.ToString().Trim();
    }

    [GeneratedRegex(@"^#\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex TitleRegex();
}

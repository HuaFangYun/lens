namespace Instancium.Lens.Chat;

/// <summary>
/// A grounded chat answer with its citation list.
/// </summary>
public sealed record ChatAnswer
{
    /// <summary>
    /// Initializes an answer with required fields.
    /// </summary>
    public ChatAnswer(string text, IReadOnlyList<Citation> citations)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(citations);

        Text = text;
        Citations = citations;
    }

    /// <summary>Generated answer text containing [n] markers.</summary>
    public string Text { get; }

    /// <summary>Citations ordered by marker number.</summary>
    public IReadOnlyList<Citation> Citations { get; }
}

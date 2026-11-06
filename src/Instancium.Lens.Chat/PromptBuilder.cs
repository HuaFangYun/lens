using System.Text;

namespace Instancium.Lens.Chat;

/// <summary>
/// Builds the RAG prompt: numbered grounded context plus the user question.
/// Marker numbering here defines the citation numbering returned by ChatService;
/// both must iterate the same collection in the same order.
/// </summary>
public static class PromptBuilder
{
    /// <summary>
    /// Builds a grounded prompt with [n] citation markers.
    /// </summary>
    /// <param name="context">Retrieved chunks, non-empty.</param>
    /// <param name="question">User question.</param>
    /// <returns>Assembled prompt text.</returns>
    /// <exception cref="ArgumentException">Context is empty or question is blank.</exception>
    public static string Build(IReadOnlyList<RetrievedChunk> context, string question)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        if (context.Count == 0)
        {
            throw new ArgumentException("Context cannot be empty.", nameof(context));
        }

        var builder = new StringBuilder();
        builder.AppendLine(
            "You are a precise assistant answering questions about the user's documents.");
        builder.AppendLine("Rules:");
        builder.AppendLine("- Use ONLY the context below. Do not invent facts.");
        builder.AppendLine(
            "- Mark every factual statement with the [n] marker of the context item it came from.");
        builder.AppendLine(
            "- If the context does not contain the answer, say that the indexed documents do not contain it.");
        builder.AppendLine();
        builder.AppendLine("Context:");

        for (var i = 0; i < context.Count; i++)
        {
            var chunk = context[i];
            builder.AppendLine(
                $"[{i + 1}] source: {chunk.Source}, chunk: {chunk.ChunkIndex}");
            builder.AppendLine(chunk.Text);
            builder.AppendLine();
        }

        builder.AppendLine($"Question: {question.Trim()}");
        builder.AppendLine("Answer:");
        return builder.ToString();
    }
}

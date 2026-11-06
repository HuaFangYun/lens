namespace Instancium.Lens.Chat;

/// <summary>
/// MVP query builder: whitespace-normalized pass-through.
/// Exists as a named seam so Phase 5 history rewriting does not touch ChatService.
/// </summary>
public sealed class QueryBuilder : IQueryBuilder
{
    /// <inheritdoc />
    public string Build(string question)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        return question.Trim();
    }
}

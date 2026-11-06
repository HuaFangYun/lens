namespace Instancium.Lens.Chat;

/// <summary>
/// Builds the retrieval query from user input.
/// </summary>
public interface IQueryBuilder
{
    /// <summary>
    /// Produces the query text sent to the vector index.
    /// MVP is an identity transform; multi-turn history rewriting plugs in here (Phase 5).
    /// </summary>
    /// <param name="question">Raw user question.</param>
    /// <returns>Query text for retrieval.</returns>
    string Build(string question);
}

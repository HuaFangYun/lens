namespace Instancium.Lens.App;

/// <summary>
/// Shared session state across pages. The profile id doubles as the Vectra collection
/// name, so indexing and querying must always agree on it.
/// </summary>
public sealed class AppState
{
    /// <summary>Active profile identifier (Vectra collection name).</summary>
    public string ProfileId { get; set; } = "default";
}

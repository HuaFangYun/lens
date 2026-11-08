namespace Instancium.Lens.App.Services;

/// <summary>
/// Resolved Vectra storage root. WHY: §4.0 DoD item 2 — the user must always see
/// where the index physically lives; computed once at the composition root so the
/// UI and the engine can never disagree on the path.
/// </summary>
public sealed record VectraStorageInfo(string BaseDirectory);

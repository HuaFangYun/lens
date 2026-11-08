namespace Instancium.Lens.Chat;

/// <summary>Pipeline stage currently executing.</summary>
public enum ChatStage
{
    Retrieving = 0,
    Generating = 1
}

/// <summary>
/// Observable pipeline stage plus an incremental answer token.
/// WHY: §4.0 DoD item 3 — the UI must never sit in an opaque "Thinking…" state;
/// stages and tokens are the only progress signal the chat page renders.
/// </summary>
public sealed record ChatProgress(ChatStage Stage, string? Token);

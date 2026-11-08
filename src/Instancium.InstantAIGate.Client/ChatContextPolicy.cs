namespace Instancium.InstantAIGate.Client;

/// <summary>
/// Context retention policy applied when a chat client instance is created.
/// WHY: Lens single-shot Q&A must not occupy VRAM session slots or retain KV-cache,
/// while multi-turn consumers need the opposite; the policy is a creation-time
/// decision, not a per-call flag.
/// </summary>
public enum ChatContextPolicy
{
    /// <summary>One question, one answer: session is hard-destroyed after each interaction.</summary>
    Stateless = 0,

    /// <summary>Multi-turn dialogue: KV-cache retained across interactions until the client is disposed.</summary>
    Conversational = 1
}

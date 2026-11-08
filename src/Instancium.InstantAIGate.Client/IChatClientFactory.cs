namespace Instancium.InstantAIGate.Client;

/// <summary>Creates chat clients bound to a context policy at creation time.</summary>
public interface IChatClientFactory
{
    /// <summary>Creates a client whose session lifecycle follows the given policy.</summary>
    IChatClient Create(ChatContextPolicy policy);
}

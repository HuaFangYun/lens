namespace Instancium.Lens.Core;

/// <summary>
/// Machine-readable description of a failure.
/// Failures travel as values, not exceptions, so integration adapters (Sprint 3-4)
/// can report remote faults without stack unwinding on hot paths.
/// </summary>
public sealed record LensError
{
    /// <summary>
    /// Initializes an error; both identifiers are mandatory to keep logs correlatable.
    /// </summary>
    /// <param name="code">Stable machine-readable identifier, e.g. vectra.timeout.</param>
    /// <param name="message">Human-readable description; must never be parsed by code.</param>
    /// <exception cref="ArgumentException">Either argument is null or white space.</exception>
    public LensError(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Message = message;
    }

    /// <summary>Stable machine-readable identifier of the failure.</summary>
    public string Code { get; }

    /// <summary>Human-readable description of the failure.</summary>
    public string Message { get; }
}

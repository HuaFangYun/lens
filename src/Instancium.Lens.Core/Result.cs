namespace Instancium.Lens.Core;

/// <summary>
/// Outcome of an operation that returns no value.
/// Default instance is a success, which makes accidental uninitialized state safe.
/// </summary>
public readonly struct Result
{
    private Result(LensError? error) => Error = error;

    /// <summary>The failure description; null when the operation succeeded.</summary>
    public LensError? Error { get; }

    /// <summary>True when the operation completed without a failure.</summary>
    public bool IsSuccess => Error is null;

    /// <summary>True when the operation produced a failure.</summary>
    public bool IsFailure => Error is not null;

    /// <summary>Creates a successful outcome.</summary>
    public static Result Success() => default;

    /// <summary>Creates a failed outcome.</summary>
    /// <exception cref="ArgumentNullException">error is null.</exception>
    public static Result Failure(LensError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(error);
    }

    /// <inheritdoc />
    public override string ToString() => IsSuccess ? "Success" : $"Failure({Error!.Code})";
}

/// <summary>
/// Outcome of an operation that returns a value.
/// </summary>
/// <typeparam name="TValue">Type of the produced value.</typeparam>
public readonly struct Result<TValue>
{
    private Result(TValue? value, LensError? error)
    {
        Value = value;
        Error = error;
    }

    /// <summary>The produced value; default when the operation failed.</summary>
    public TValue? Value { get; }

    /// <summary>The failure description; null when the operation succeeded.</summary>
    public LensError? Error { get; }

    /// <summary>True when the operation completed without a failure.</summary>
    public bool IsSuccess => Error is null;

    /// <summary>True when the operation produced a failure.</summary>
    public bool IsFailure => Error is not null;

    /// <summary>Creates a successful outcome; null payload is a programming error.</summary>
    /// <exception cref="ArgumentNullException">value is null.</exception>
    public static Result<TValue> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<TValue>(value, null);
    }

    /// <summary>Creates a failed outcome.</summary>
    /// <exception cref="ArgumentNullException">error is null.</exception>
    public static Result<TValue> Failure(LensError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<TValue>(default, error);
    }

    /// <summary>Returns the value or throws when the outcome is a failure.</summary>
    /// <exception cref="InvalidOperationException">The outcome is a failure.</exception>
    public TValue ValueOrThrow() => IsSuccess
        ? Value!
        : throw new InvalidOperationException($"Result is failure: {Error!.Code} {Error.Message}");

    /// <summary>Drops the value, keeping only the outcome status.</summary>
    public Result AsResult() => IsSuccess ? Result.Success() : Result.Failure(Error!);

    /// <inheritdoc />
    public override string ToString() => IsSuccess ? $"Success({Value})" : $"Failure({Error!.Code})";
}

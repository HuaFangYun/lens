using Instancium.Lens.Core;
using Xunit;

namespace Instancium.Lens.Core.Tests;

public sealed class LensErrorTests
{
    [Fact]
    public void Ctor_RejectsWhiteSpaceCode() =>
        Assert.Throws<ArgumentException>(() => new LensError(" ", "message"));

    [Fact]
    public void Ctor_RejectsNullMessage() =>
        Assert.Throws<ArgumentNullException>(() => new LensError("code", null!));
}

public sealed class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_KeepsError()
    {
        var result = Result.Failure(new LensError("core.timeout", "timed out"));

        Assert.True(result.IsFailure);
        Assert.Equal("core.timeout", result.Error!.Code);
    }

    [Fact]
    public void Failure_RejectsNullError() =>
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
}

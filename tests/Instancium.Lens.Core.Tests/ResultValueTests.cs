using Instancium.Lens.Core;
using Xunit;

namespace Instancium.Lens.Core.Tests;

public sealed class ResultValueTests
{
    [Fact]
    public void Success_ReturnsValue()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Success_RejectsNullValue() =>
        Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));

    [Fact]
    public void ValueOrThrow_ThrowsOnFailure()
    {
        var result = Result<int>.Failure(new LensError("core.timeout", "timed out"));

        var exception = Assert.Throws<InvalidOperationException>(() => result.ValueOrThrow());
        Assert.Contains("core.timeout", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AsResult_PropagatesSuccess() =>
        Assert.True(Result<int>.Success(1).AsResult().IsSuccess);

    [Fact]
    public void AsResult_PropagatesFailure()
    {
        var result = Result<int>.Failure(new LensError("vectra.unreachable", "no route"));

        var plain = result.AsResult();

        Assert.True(plain.IsFailure);
        Assert.Equal("vectra.unreachable", plain.Error!.Code);
    }
}

public sealed class SolutionSmokeTests
{
    // Guards the release baseline: version must come from Directory.Build.props, not csproj.
    [Fact]
    public void CoreAssembly_Version_MatchesReleaseBaseline()
    {
        var version = typeof(Result).Assembly.GetName().Version;

        Assert.Equal(new Version(0, 1, 0, 0), version);
    }
}

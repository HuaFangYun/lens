using Xunit;

namespace Instancium.Lens.Chat.Tests;

public sealed class QueryBuilderTests
{
    [Fact]
    public void Build_TrimsWhitespace() =>
        Assert.Equal("what is this", new QueryBuilder().Build("  what is this  "));

    [Fact]
    public void Build_RejectsBlankQuestion() =>
        Assert.Throws<ArgumentException>(() => new QueryBuilder().Build("   "));
}

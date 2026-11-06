using Xunit;

namespace Instancium.Lens.Chat.Tests;

public sealed class PromptBuilderTests
{
    [Fact]
    public void Build_NumbersContextAndIncludesQuestion()
    {
        var context = new[]
        {
            new RetrievedChunk("a.md", 0, "first chunk"),
            new RetrievedChunk("b.txt", 4, "second chunk")
        };

        var prompt = PromptBuilder.Build(context, "What is inside?");

        Assert.Contains("[1] source: a.md, chunk: 0", prompt);
        Assert.Contains("first chunk", prompt);
        Assert.Contains("[2] source: b.txt, chunk: 4", prompt);
        Assert.Contains("second chunk", prompt);
        Assert.Contains("Question: What is inside?", prompt);
        Assert.Contains("Answer:", prompt);
    }

    [Fact]
    public void Build_EmptyContext_Throws() =>
        Assert.Throws<ArgumentException>(
            () => PromptBuilder.Build(Array.Empty<RetrievedChunk>(), "q"));

    [Fact]
    public void Build_BlankQuestion_Throws() =>
        Assert.Throws<ArgumentException>(
            () => PromptBuilder.Build([new RetrievedChunk("a.md", 0, "t")], " "));
}

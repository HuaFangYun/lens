using Xunit;

namespace Instancium.Lens.Chat.Tests;

public sealed class ChatOptionsTests
{
    [Fact]
    public void Defaults_ToTopK5() =>
        Assert.Equal(5, new ChatOptions().TopK);

    [Fact]
    public void Validate_RejectsNonPositiveTopK() =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ChatOptions { TopK = 0 }.Validate());
}

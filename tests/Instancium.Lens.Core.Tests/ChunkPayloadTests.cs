using System.Text.Json;
using Instancium.Lens.Core;
using Xunit;

namespace Instancium.Lens.Core.Tests;

public sealed class ChunkPayloadTests
{
    [Fact]
    public void Serialize_ProducesContractFieldNames()
    {
        var payload = new ChunkPayload
        {
            Version = 1,
            Text = "body",
            Source = "notes/a.md",
            ChunkIndex = 3
        };

        var json = JsonSerializer.Serialize(payload);

        Assert.Equal(
            "{\"v\":1,\"text\":\"body\",\"source\":\"notes/a.md\",\"chunkIndex\":3}",
            json);
    }

    [Fact]
    public void Deserialize_RoundTripsContract()
    {
        const string json =
            "{\"v\":1,\"text\":\"body\",\"source\":\"notes/a.md\",\"chunkIndex\":3}";

        var payload = JsonSerializer.Deserialize<ChunkPayload>(json);

        Assert.NotNull(payload);
        Assert.Equal(1, payload!.Version);
        Assert.Equal("body", payload.Text);
        Assert.Equal("notes/a.md", payload.Source);
        Assert.Equal(3, payload.ChunkIndex);
    }

    [Fact]
    public void Deserialize_ToleratesUnknownFutureFields()
    {
        const string json =
            "{\"v\":2,\"text\":\"body\",\"source\":\"a.md\",\"chunkIndex\":0,\"page\":5}";

        var payload = JsonSerializer.Deserialize<ChunkPayload>(json);

        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Version);
    }
}

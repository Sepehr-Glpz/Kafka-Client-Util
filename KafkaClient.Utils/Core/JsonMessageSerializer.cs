using System.IO;
using System.Text.Json;
using System.Threading;

namespace KafkaClient.Utils.Core;
internal class JsonMessageSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);
    public byte[] Serialize<TMessage>(TMessage message)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, message, _options);
        return stream.ToArray();
    }

    public async Task<byte[]> SerializeAsync<TMessage>(TMessage message, CancellationToken ct = default)
    {
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, message, _options, ct);
        return stream.ToArray();
    }

    public TMessage? Deserialize<TMessage>(byte[] data)
    {
        return JsonSerializer.Deserialize<TMessage>(data, _options);
    }

    public Task<TMessage?> DeserializeAsync<TMessage>(byte[] data, CancellationToken ct = default)
    {
        using var stream = new MemoryStream(data);
        return JsonSerializer.DeserializeAsync<TMessage>(stream, _options, ct).AsTask();
    }

    public static JsonMessageSerializer Instance = new();
}

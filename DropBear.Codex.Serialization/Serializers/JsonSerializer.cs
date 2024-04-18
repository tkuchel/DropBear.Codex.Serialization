using System.Text.Json;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Serializers;

public class JsonSerializer : ISerializer
{
    private readonly RecyclableMemoryStreamManager _memoryManager;
    private readonly JsonSerializerOptions _options;

    public JsonSerializer(JsonSerializerOptions options, RecyclableMemoryStreamManager memoryManager)
    {
        _options = options;
        _memoryManager = memoryManager;
    }

    public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        var memoryStream = new RecyclableMemoryStream(_memoryManager);
        await using (memoryStream.ConfigureAwait(false))
        {
            await System.Text.Json.JsonSerializer.SerializeAsync(memoryStream, value, _options, cancellationToken)
                .ConfigureAwait(false);
            return memoryStream.ToArray();
        }
    }

    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
    {
        var memoryStream = new RecyclableMemoryStream(_memoryManager);
        await using (memoryStream.ConfigureAwait(false))
        {
            await memoryStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(memoryStream, _options, cancellationToken)
                .ConfigureAwait(false) ?? default(T);
        }
    }
}

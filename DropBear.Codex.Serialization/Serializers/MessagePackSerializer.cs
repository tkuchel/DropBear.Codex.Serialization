using DropBear.Codex.Serialization.Interfaces;
using MessagePack;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Serializers;

public class MessagePackSerializer : ISerializer
{
    private readonly RecyclableMemoryStreamManager _memoryManager;
    private readonly MessagePackSerializerOptions _options;

    public MessagePackSerializer(MessagePackSerializerOptions options, RecyclableMemoryStreamManager memoryManager)
    {
        _options = options;
        _memoryManager = memoryManager;
    }

    public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        var memoryStream = new RecyclableMemoryStream(_memoryManager);
        await using (memoryStream.ConfigureAwait(false))
        {
            await MessagePack.MessagePackSerializer.SerializeAsync(memoryStream, value, _options, cancellationToken)
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
            return await MessagePack.MessagePackSerializer
                .DeserializeAsync<T>(memoryStream, _options, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

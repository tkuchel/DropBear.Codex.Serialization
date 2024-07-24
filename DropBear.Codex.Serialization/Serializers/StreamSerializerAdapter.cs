#region

using DropBear.Codex.Serialization.Interfaces;

#endregion

namespace DropBear.Codex.Serialization.Serializers;

/// <summary>
///     Adapter that allows an IStreamSerializer to be used where an ISerializer is expected.
/// </summary>
public class StreamSerializerAdapter : ISerializer
{
    private readonly IStreamSerializer _streamSerializer;

    public StreamSerializerAdapter(IStreamSerializer streamSerializer)
    {
        _streamSerializer = streamSerializer;
    }

    public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await _streamSerializer.SerializeAsync(memoryStream, value, cancellationToken).ConfigureAwait(false);
        return memoryStream.ToArray();
    }

    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream(data);
        return await _streamSerializer.DeserializeAsync<T>(memoryStream, cancellationToken).ConfigureAwait(false);
    }
}

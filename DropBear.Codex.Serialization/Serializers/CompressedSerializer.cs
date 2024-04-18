using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Serializers;

public class CompressedSerializer : ISerializer
{
    private readonly ISerializer _innerSerializer;
    private readonly ICompressor _compressor;

    public CompressedSerializer(ISerializer innerSerializer, ICompressor compressor)
    {
        _innerSerializer = innerSerializer;
        _compressor = compressor;
    }

    public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        var serializedData = await _innerSerializer.SerializeAsync(value, cancellationToken).ConfigureAwait(false);
        return await _compressor.CompressAsync(serializedData, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
    {
        var decompressedData = await _compressor.DecompressAsync(data, cancellationToken).ConfigureAwait(false);
        return await _innerSerializer.DeserializeAsync<T>(decompressedData, cancellationToken).ConfigureAwait(false);
    }
}

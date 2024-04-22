using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Serializers;

/// <summary>
///     Serializer that applies compression to serialized data before serialization and decompression after
///     deserialization.
/// </summary>
public class CompressedSerializer : ISerializer
{
    private readonly ICompressor _compressor;
    private readonly ISerializer _innerSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompressedSerializer" /> class.
    /// </summary>
    /// <param name="innerSerializer">The inner serializer.</param>
    /// <param name="compressor">The compressor to use for compression and decompression.</param>
    public CompressedSerializer(ISerializer innerSerializer, ICompressor compressor)
    {
        _innerSerializer = innerSerializer;
        _compressor = compressor;
    }

    /// <inheritdoc />
    public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        var serializedData = await _innerSerializer.SerializeAsync(value, cancellationToken).ConfigureAwait(false);
        return await _compressor.CompressAsync(serializedData, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<T?> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
    {
        var decompressedData = await _compressor.DecompressAsync(data, cancellationToken).ConfigureAwait(false);
        return await _innerSerializer.DeserializeAsync<T>(decompressedData, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Deserializes raw byte data after decompressing it, returning the decompressed byte array.
    /// </summary>
    /// <param name="data">The compressed byte data to deserialize.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the deserialization operation.</param>
    /// <returns>The decompressed byte array from the provided compressed data.</returns>
    public async Task<byte[]> DeserializeRawBytesAsync(byte[] data, CancellationToken cancellationToken = default) =>
        await _compressor.DecompressAsync(data, cancellationToken).ConfigureAwait(false);
}

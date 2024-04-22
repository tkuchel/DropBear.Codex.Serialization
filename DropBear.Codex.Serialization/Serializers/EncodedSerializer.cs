using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Serializers;

/// <summary>
///     Serializer that applies encoding to serialized data before serialization and decoding after deserialization.
/// </summary>
public class EncodedSerializer : ISerializer
{
    private readonly IEncoder _encoder;
    private readonly ISerializer _innerSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EncodedSerializer" /> class.
    /// </summary>
    /// <param name="innerSerializer">The inner serializer.</param>
    /// <param name="encodingProvider">The encoding provider to use for encoding and decoding.</param>
    public EncodedSerializer(ISerializer innerSerializer, IEncodingProvider encodingProvider)
    {
        _innerSerializer = innerSerializer;
        _encoder = encodingProvider.GetEncoder();
    }

    /// <inheritdoc />
    public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        // Serialize the value using the inner serializer
        var serializedData = await _innerSerializer.SerializeAsync(value, cancellationToken).ConfigureAwait(false);

        // Encode the serialized data using the provided encoder
        return await _encoder.EncodeAsync(serializedData, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<T?> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
    {
        // Decode the data using the provided encoder
        var decodedData = await _encoder.DecodeAsync(data, cancellationToken).ConfigureAwait(false);

        // Deserialize the decoded data using the inner serializer
        return await _innerSerializer.DeserializeAsync<T>(decodedData, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Deserializes raw byte data after decoding it, returning the decoded byte array.
    /// </summary>
    /// <param name="data">The encoded byte data to deserialize.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the deserialization operation.</param>
    /// <returns>The decoded byte array from the provided encoded data.</returns>
    public async Task<byte[]> DeserializeRawBytesAsync(byte[] data, CancellationToken cancellationToken = default) =>
        // Decode the data using the provided encoder
        await _encoder.DecodeAsync(data, cancellationToken).ConfigureAwait(false);
}

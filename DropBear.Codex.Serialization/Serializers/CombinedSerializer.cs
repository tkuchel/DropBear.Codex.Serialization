using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Serializers;

/// <summary>
///     A serializer that combines both regular and stream-based serialization strategies.
///     It chooses the appropriate strategy based on the context or type of the data.
/// </summary>
public class CombinedSerializer : ISerializer
{
    private const int LargeSizeThreshold = 1024 * 1024 * 10;
    private readonly ISerializer _defaultSerializer;
    private readonly IStreamSerializer _streamSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CombinedSerializer" /> class.
    /// </summary>
    /// <param name="defaultSerializer">The default serializer to use for non-stream-based serialization.</param>
    /// <param name="streamSerializer">The stream-based serializer for handling stream input/output.</param>
    public CombinedSerializer(ISerializer defaultSerializer, IStreamSerializer streamSerializer)
    {
        _defaultSerializer = defaultSerializer ?? throw new ArgumentNullException(nameof(defaultSerializer));
        _streamSerializer = streamSerializer ?? throw new ArgumentNullException(nameof(streamSerializer));
    }

    /// <summary>
    ///     Serializes an object asynchronously, using either the default serializer or the stream serializer depending on the
    ///     type of the value.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and returns the serialized data as a byte array.</returns>
    public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
    {
        // Example condition: Use stream serializer if T is Stream or similar logic
        if (value is not Stream streamValue)
            return await _defaultSerializer.SerializeAsync(value, cancellationToken).ConfigureAwait(false);
        var memoryStream = new MemoryStream();
        await _streamSerializer.SerializeAsync(memoryStream, streamValue, cancellationToken).ConfigureAwait(false);
        return memoryStream.ToArray();
    }

    /// <summary>
    ///     Deserializes data asynchronously, using either the default serializer or the stream serializer depending on the
    ///     type of the data.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="data">The data to deserialize.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and returns the deserialized object.</returns>
    public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
    {
        // Example condition: Use stream serializer if expected type is Stream or based on data size, etc.
        if (typeof(T) != typeof(Stream) && data.Length <= LargeSizeThreshold)
            return await _defaultSerializer.DeserializeAsync<T>(data, cancellationToken).ConfigureAwait(false);
        var memoryStream = new MemoryStream(data);
        return await _streamSerializer.DeserializeAsync<T>(memoryStream, cancellationToken).ConfigureAwait(false);
    }
}

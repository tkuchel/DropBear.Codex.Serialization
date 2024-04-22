namespace DropBear.Codex.Serialization.Interfaces;

/// <summary>
///     Defines methods for stream-based serialization and deserialization.
///     This interface can handle direct stream inputs and outputs for serialization,
///     which is ideal for large data processing or when data should not be fully loaded into memory.
/// </summary>
public interface IStreamSerializer
{
    /// <summary>
    ///     Serializes an object to a stream asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="stream">The stream to serialize the object to.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the serialization process.</param>
    /// <returns>A task that represents the asynchronous serialization operation.</returns>
    Task SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deserializes an object from a stream asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="stream">The stream to deserialize the object from.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the deserialization process.</param>
    /// <returns>A task that represents the asynchronous deserialization operation and yields the deserialized object.</returns>
    Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously deserializes a stream into its original format, applying necessary decryption
    /// and decompression as configured. This method is intended to handle byte arrays that might have been
    /// altered or wrapped with additional processing such as encryption or compression.
    /// </summary>
    /// <param name="stream">The stream containing the data to be deserialized.</param>
    /// <param name="cancellationToken">The cancellation token to be used.</param>
    /// <returns>
    /// A task that represents the asynchronous deserialization operation. The task result contains the
    /// byte array of the deserialized data. If decryption or decompression is applied, the returned byte array
    /// represents the data in its original form prior to those processes.
    /// </returns>
    /// <remarks>
    /// Implementors must ensure that this method correctly handles the configuration for decompression
    /// and decryption as necessary. If the input data is not compressed or encrypted, this method should
    /// essentially revert any serialization applied, typically restoring the byte array to its pre-serialized state.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="stream"/> argument is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if deserialization fails due to configuration issues or
    /// if the input data is in an unrecognized format or corrupted.</exception>
    public Task<byte[]> DeserializeRawBytesAsync(Stream stream,CancellationToken cancellationToken = default);
}

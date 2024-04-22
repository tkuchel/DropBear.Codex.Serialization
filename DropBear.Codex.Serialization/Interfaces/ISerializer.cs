namespace DropBear.Codex.Serialization.Interfaces
{
    /// <summary>
    /// Interface for serializers.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Asynchronously serializes the provided value.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation, which upon completion returns the serialized data as a byte array.</returns>
        Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously deserializes the provided byte array into an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to deserialize into.</typeparam>
        /// <param name="data">The byte array containing the serialized data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation, which upon completion returns the deserialized value of type <typeparamref name="T"/>.</returns>
        Task<T?> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default);


        /// <summary>
        /// Asynchronously deserializes raw byte data into its original format, applying necessary decryption
        /// and decompression as configured. This method is intended to handle byte arrays that might have been
        /// altered or wrapped with additional processing such as encryption or compression.
        /// </summary>
        /// <param name="data">The byte array containing the data to be deserialized.</param>
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
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="data"/> argument is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if deserialization fails due to configuration issues or
        /// if the input data is in an unrecognized format or corrupted.</exception>
        public Task<byte[]> DeserializeRawBytesAsync(byte[] data,CancellationToken cancellationToken = default);

    }
}

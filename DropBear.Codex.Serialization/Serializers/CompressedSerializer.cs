using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Serializers
{
    /// <summary>
    /// Serializer that applies compression to serialized data before serialization and decompression after deserialization.
    /// </summary>
    public class CompressedSerializer : ISerializer
    {
        private readonly ISerializer _innerSerializer;
        private readonly ICompressor _compressor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedSerializer"/> class.
        /// </summary>
        /// <param name="innerSerializer">The inner serializer.</param>
        /// <param name="compressor">The compressor to use for compression and decompression.</param>
        public CompressedSerializer(ISerializer innerSerializer, ICompressor compressor)
        {
            _innerSerializer = innerSerializer;
            _compressor = compressor;
        }

        /// <inheritdoc/>
        public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
        {
            var serializedData = await _innerSerializer.SerializeAsync(value, cancellationToken).ConfigureAwait(false);
            return await _compressor.CompressAsync(serializedData, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
        {
            var decompressedData = await _compressor.DecompressAsync(data, cancellationToken).ConfigureAwait(false);
            return await _innerSerializer.DeserializeAsync<T>(decompressedData, cancellationToken).ConfigureAwait(false);
        }
    }
}

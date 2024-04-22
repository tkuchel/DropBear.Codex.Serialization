using System.Text.Json;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Writers
{
    /// <summary>
    /// Implementation of <see cref="ISerializerReader"/> for JSON serialization.
    /// </summary>
    public class JsonSerializerReader : ISerializerReader
    {
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerReader"/> class with the specified options.
        /// </summary>
        /// <param name="options">The JSON serializer options.</param>
        public JsonSerializerReader(JsonSerializerOptions options)
        {
            _options = options;
        }

        /// <inheritdoc/>
        public async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken)
                .ConfigureAwait(false) ?? default;
        }
        
        /// <summary>
        /// Deserializes raw byte data from a stream into a byte array directly.
        /// </summary>
        /// <param name="stream">The stream containing the data to deserialize.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation and returns the byte array.</returns>
        public static async Task<byte[]> DeserializeRawBytesAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            return memoryStream.ToArray();
        }
    }
}

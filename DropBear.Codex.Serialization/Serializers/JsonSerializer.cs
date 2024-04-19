using System.Text.Json;
using DropBear.Codex.Serialization.Exceptions;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Serializers
{
    /// <summary>
    /// Serializer implementation for JSON serialization and deserialization.
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        private readonly RecyclableMemoryStreamManager _memoryManager;
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializer"/> class.
        /// </summary>
        /// <param name="options">The JSON serialization options.</param>
        /// <param name="memoryManager">The memory manager for recyclable memory streams.</param>
        public JsonSerializer(JsonSerializerOptions options, RecyclableMemoryStreamManager memoryManager)
        {
            _options = options;
            _memoryManager = memoryManager;
        }

        /// <inheritdoc/>
        public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
        {
            var memoryStream = new RecyclableMemoryStream(_memoryManager);
            await using (memoryStream.ConfigureAwait(false))
            {
                await System.Text.Json.JsonSerializer.SerializeAsync(memoryStream, value, _options, cancellationToken)
                    .ConfigureAwait(false);
                return memoryStream.ToArray();
            }
        }

        /// <inheritdoc/>
        public async Task<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
        {
            var memoryStream = new RecyclableMemoryStream(_memoryManager);
            await using (memoryStream.ConfigureAwait(false))
            {
                await memoryStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var result = await System.Text.Json.JsonSerializer
                    .DeserializeAsync<T>(memoryStream, _options, cancellationToken)
                    .ConfigureAwait(false) ?? throw new DeserializationException("Failed to deserialize data");
                return result;
            }
        }
    }
}

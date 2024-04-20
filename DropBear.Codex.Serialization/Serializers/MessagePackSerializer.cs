using DropBear.Codex.Serialization.Exceptions;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Serializers
{
    /// <summary>
    /// Serializer implementation for MessagePack serialization and deserialization.
    /// </summary>
    public class MessagePackSerializer : ISerializer
    {
        private readonly RecyclableMemoryStreamManager _memoryManager;
        private readonly MessagePackSerializerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePackSerializer"/> class.
        /// </summary>
        /// <param name="options">The MessagePack serialization options.</param>
        /// <param name="memoryManager">The memory manager for recyclable memory streams.</param>
        public MessagePackSerializer(MessagePackSerializerOptions options, RecyclableMemoryStreamManager memoryManager)
        {
            _options = options;
            _memoryManager = memoryManager;
        }

        /// <inheritdoc/>
        public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken cancellationToken = default)
        {
            try
            {
                var memoryStream = new RecyclableMemoryStream(_memoryManager);
                await using (memoryStream.ConfigureAwait(false))
                {
                    await MessagePack.MessagePackSerializer
                        .SerializeAsync(memoryStream, value, _options, cancellationToken)
                        .ConfigureAwait(false);
                    return memoryStream.ToArray();
                }
            }
            catch(MessagePackSerializationException ex)
            {
                if(ex.InnerException is FormatterNotRegisteredException)
                    throw new SerializationException("Error occurred while serializing data. Ensure all types are registered.", ex);
                throw new SerializationException("Error occurred while serializing data.", ex);
            }
            catch (Exception ex)
            {
                throw new SerializationException("Error occurred while serializing data.", ex);
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
                return await MessagePack.MessagePackSerializer
                    .DeserializeAsync<T>(memoryStream, _options, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}

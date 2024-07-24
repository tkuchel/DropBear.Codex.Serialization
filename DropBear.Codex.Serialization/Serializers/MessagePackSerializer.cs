#region

using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Exceptions;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;
using Microsoft.IO;

#endregion

namespace DropBear.Codex.Serialization.Serializers;

/// <summary>
///     Serializer implementation for MessagePack serialization and deserialization.
/// </summary>
public class MessagePackSerializer : ISerializer
{
    private readonly RecyclableMemoryStreamManager _memoryManager;
    private readonly MessagePackSerializerOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessagePackSerializer" /> class.
    /// </summary>
    public MessagePackSerializer(SerializationConfig config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config), "Configuration must be provided.");
        }

        _options = config.MessagePackSerializerOptions ?? MessagePackSerializerOptions.Standard;
#pragma warning disable CA2208
        _memoryManager = config.RecyclableMemoryStreamManager ?? throw new ArgumentNullException(
#pragma warning disable MA0015
            nameof(config.RecyclableMemoryStreamManager), "RecyclableMemoryStreamManager must be provided.");
#pragma warning restore CA2208
#pragma warning restore MA0015
    }

    /// <inheritdoc />
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
        catch (MessagePackSerializationException ex)
        {
            if (ex.InnerException is FormatterNotRegisteredException)
            {
                throw new SerializationException(
                    "Error occurred while serializing data. Ensure all types are registered.", ex);
            }

            throw new SerializationException("Error occurred while serializing data.", ex);
        }
        catch (Exception ex)
        {
            throw new SerializationException("Error occurred while serializing data.", ex);
        }
    }

    /// <inheritdoc />
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

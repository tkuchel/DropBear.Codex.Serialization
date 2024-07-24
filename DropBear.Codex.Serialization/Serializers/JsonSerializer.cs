#region

using System.Text.Json;
using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Exceptions;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

#endregion

namespace DropBear.Codex.Serialization.Serializers;

/// <summary>
///     Serializer implementation for JSON serialization and deserialization.
/// </summary>
public class JsonSerializer : ISerializer
{
    private readonly RecyclableMemoryStreamManager _memoryManager;
    private readonly JsonSerializerOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonSerializer" /> class.
    /// </summary>
    public JsonSerializer(SerializationConfig config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config), "Configuration must be provided.");
        }

        _options = config.JsonSerializerOptions ?? new JsonSerializerOptions();
#pragma warning disable CA2208
        _memoryManager = config.RecyclableMemoryStreamManager ?? throw new ArgumentNullException(
#pragma warning disable MA0015
            nameof(config.RecyclableMemoryStreamManager), "RecyclableMemoryStreamManager must be provided.");
#pragma warning restore MA0015
#pragma warning restore CA2208
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

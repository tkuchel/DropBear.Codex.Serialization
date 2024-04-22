using System.Text.Json;
using DropBear.Codex.Serialization.Exceptions;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

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
    /// <param name="options">The JSON serialization options.</param>
    /// <param name="memoryManager">The memory manager for recyclable memory streams.</param>
    public JsonSerializer(JsonSerializerOptions options, RecyclableMemoryStreamManager memoryManager)
    {
        _options = options;
        _memoryManager = memoryManager;
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
    public async Task<T?> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
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

    /// <summary>
    ///     Deserializes raw byte data into the specified type, applying JSON deserialization settings.
    /// </summary>
    /// <param name="data">The raw byte data to deserialize.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>The deserialized object from the provided raw byte data.</returns>
    public async Task<byte[]> DeserializeRawBytesAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        var memoryStream = new RecyclableMemoryStream(_memoryManager);
        await using (memoryStream.ConfigureAwait(false))
        {
            await memoryStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Assume the data is a simple JSON string that represents a byte array, and deserialize it directly.
            // Adjust the logic here if the JSON data structure is different.
            return await System.Text.Json.JsonSerializer
                       .DeserializeAsync<byte[]>(memoryStream, _options, cancellationToken).ConfigureAwait(false)
                   ?? throw new InvalidOperationException("Failed to deserialize raw bytes.");
        }
    }
}

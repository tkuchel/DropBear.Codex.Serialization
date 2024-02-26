using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Implements serialization and deserialization for various data formats, including JSON, MessagePack, and MemoryPack.
///     Supports optional compression and encoding for JSON data format.
/// </summary>
public class DataSerializer : IDataSerializer
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<DataSerializer> _logger;
    private readonly ISerializableChecker _memoryPackChecker;
    private readonly IMemoryPackSerializer _memoryPackSerializer;
    private readonly ISerializableChecker _messagePackChecker;
    private readonly IMessagePackSerializer _messagePackSerializer;

    public DataSerializer(ILogger<DataSerializer> logger, IJsonSerializer jsonSerializer,
        IMessagePackSerializer messagePackSerializer, IMemoryPackSerializer memoryPackSerializer,
        Func<string, ISerializableChecker> checkerFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        _jsonSerializer = jsonSerializer ??
                          throw new ArgumentNullException(nameof(jsonSerializer), "JSON Serializer cannot be null.");
        _messagePackSerializer = messagePackSerializer ??
                                 throw new ArgumentNullException(nameof(messagePackSerializer),
                                     "MessagePack Serializer cannot be null.");
        _memoryPackSerializer = memoryPackSerializer ??
                                throw new ArgumentNullException(nameof(memoryPackSerializer),
                                    "MemoryPack Serializer cannot be null.");

        _messagePackChecker = checkerFactory("MessagePack") ??
                              throw new ArgumentNullException(nameof(checkerFactory),
                                  "MessagePack Checker cannot be null.");
        _memoryPackChecker = checkerFactory("MemoryPack") ??
                             throw new ArgumentNullException(nameof(checkerFactory),
                                 "MemoryPack Checker cannot be null.");
    }

    /// <inheritdoc />
    public Task<Result<string>> SerializeJsonAsync<T>(T data, CompressionOption compressionOption,
        EncodingOption encodingOption)
    {
        // Directly return Task without unnecessary await
        return _jsonSerializer.SerializeAsync(data, compressionOption, encodingOption);
    }

    /// <inheritdoc />
    public Task<Result<T>> DeserializeJsonAsync<T>(string data, CompressionOption compressionOption,
        EncodingOption encodingOption)
    {
        // Directly return Task without unnecessary await
        return _jsonSerializer.DeserializeAsync<T>(data, compressionOption, encodingOption);
    }

    /// <inheritdoc />
    public Task<Result<byte[]>> SerializeMessagePackAsync<T>(T data, CompressionOption compressionOption)
    {
        // Directly return Task without unnecessary await
        return _messagePackSerializer.SerializeAsync(data, compressionOption);
    }

    /// <inheritdoc />
    public Task<Result<T>> DeserializeMessagePackAsync<T>(byte[] data, CompressionOption compressionOption)
    {
        // Directly return Task without unnecessary await
        return _messagePackSerializer.DeserializeAsync<T>(data, compressionOption);
    }

    /// <summary>
    ///     Serializes data to a MemoryPack byte array, with optional compression. Utilizes MemoryPack's built-in compression
    ///     mechanism if supported.
    /// </summary>
    /// <param name="data">The data to serialize.</param>
    /// <param name="compressionOption">The compression option to apply.</param>
    /// <returns>A task that represents the asynchronous operation, containing the serialized data as a byte array.</returns>
    public Task<Result<byte[]>> SerializeMemoryPackAsync<T>(T data, CompressionOption compressionOption)
    {
        // Directly return Task without unnecessary await
        return _memoryPackSerializer.SerializeAsync(data, compressionOption);
    }

    /// <summary>
    ///     Deserializes a MemoryPack byte array to the specified type, using MemoryPack's built-in decompression if supported.
    /// </summary>
    /// <param name="data">The byte array to deserialize.</param>
    /// <param name="compressionOption">The compression option that was applied during serialization.</param>
    /// <returns>A task that represents the asynchronous operation, containing the deserialized object of type T.</returns>
    public Task<Result<T>> DeserializeMemoryPackAsync<T>(byte[] data, CompressionOption compressionOption)
    {
        // Directly return Task without unnecessary await
        return _memoryPackSerializer.DeserializeAsync<T>(data, compressionOption);
    }

    /// <inheritdoc />
    public Task<Result<bool>> IsMessagePackSerializable<T>() where T : class
    {
        var result = _messagePackChecker.IsSerializable<T>();
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<Result<bool>> IsMemoryPackSerializable<T>() where T : class
    {
        var result = _memoryPackChecker.IsSerializable<T>();
        return Task.FromResult(result);
    }
}
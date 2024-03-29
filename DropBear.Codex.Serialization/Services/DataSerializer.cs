using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Implements serialization and deserialization for various data formats, including JSON, MessagePack, and MemoryPack.
///     Supports optional compression and encoding for JSON data format.
/// </summary>
public class DataSerializer : IDataSerializer
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ISerializableChecker _memoryPackChecker;
    private readonly IMemoryPackSerializer _memoryPackSerializer;
    private readonly ISerializableChecker _messagePackChecker;
    private readonly IMessagePackSerializer _messagePackSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataSerializer" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="messagePackSerializer">The MessagePack serializer.</param>
    /// <param name="memoryPackSerializer">The MemoryPack serializer.</param>
    /// <param name="checkerFactory">A factory function to produce serializable checkers.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    public DataSerializer(
        IJsonSerializer jsonSerializer,
        IMessagePackSerializer messagePackSerializer,
        IMemoryPackSerializer memoryPackSerializer,
        Func<string, ISerializableChecker> checkerFactory)
    {
        _jsonSerializer = jsonSerializer ??
                          throw new ArgumentNullException(nameof(jsonSerializer), "JSON Serializer cannot be null.");
        _messagePackSerializer = messagePackSerializer ??
                                 throw new ArgumentNullException(nameof(messagePackSerializer),
                                     "MessagePack Serializer cannot be null.");
        _memoryPackSerializer = memoryPackSerializer ??
                                throw new ArgumentNullException(nameof(memoryPackSerializer),
                                    "MemoryPack Serializer cannot be null.");
        _memoryPackChecker = checkerFactory("MemoryPack") ?? throw new ArgumentNullException(nameof(checkerFactory),
            "MemoryPack Checker factory invocation returned null.");
        _messagePackChecker = checkerFactory("MessagePack") ?? throw new ArgumentNullException(nameof(checkerFactory),
            "MessagePack Checker factory invocation returned null.");
    }

    /// <inheritdoc />
    public Task<Result<string>?> SerializeJsonAsync<T>(T data, CompressionOption compressionOption,
        EncodingOption encodingOption) where T : notnull =>
        _jsonSerializer.SerializeAsync(data, compressionOption, encodingOption);

    /// <inheritdoc />
    public Task<Result<T>?> DeserializeJsonAsync<T>(string data, CompressionOption compressionOption,
        EncodingOption encodingOption) where T : notnull =>
        _jsonSerializer.DeserializeAsync<T>(data, compressionOption, encodingOption);

    /// <inheritdoc />
    public Task<Result<byte[]>> SerializeMessagePackAsync<T>(T data, CompressionOption compressionOption)
        where T : notnull =>
        _messagePackSerializer.SerializeAsync(data, compressionOption);

    /// <inheritdoc />
    public Task<Result<T>> DeserializeMessagePackAsync<T>(byte[]? data, CompressionOption compressionOption)
        where T : notnull =>
        _messagePackSerializer.DeserializeAsync<T>(data, compressionOption);

    /// <inheritdoc />
    public Task<Result<byte[]>> SerializeMemoryPackAsync<T>(T? data, CompressionOption compressionOption)
        where T : notnull =>
        _memoryPackSerializer.SerializeAsync(data, compressionOption);

    /// <inheritdoc />
    public Task<Result<T>> DeserializeMemoryPackAsync<T>(byte[]? data, CompressionOption compressionOption)
        where T : notnull =>
        _memoryPackSerializer.DeserializeAsync<T>(data, compressionOption);

    /// <inheritdoc />
    public Task<Result> IsMessagePackSerializable<T>() where T : class =>
        Task.FromResult(_messagePackChecker.IsSerializable<T>());

    /// <inheritdoc />
    public Task<Result> IsMemoryPackSerializable<T>() where T : class =>
        Task.FromResult(_memoryPackChecker.IsSerializable<T>());
}

using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;

namespace DropBear.Codex.Serialization.Interfaces;

/// <summary>
///     Provides serialization and deserialization methods for various data formats.
/// </summary>
public interface IDataSerializer
{
    /// <summary>
    ///     Serializes data to JSON format with optional compression and encoding.
    /// </summary>
    Task<Result<string>?> SerializeJsonAsync<T>(T data, CompressionOption compressionOption,
        EncodingOption encodingOption) where T : notnull;

    /// <summary>
    ///     Deserializes data from JSON format with optional compression and decoding.
    /// </summary>
    Task<Result<T>?> DeserializeJsonAsync<T>(string data, CompressionOption compressionOption,
        EncodingOption encodingOption) where T : notnull;

    /// <summary>
    ///     Serializes data to MessagePack format with optional compression.
    /// </summary>
    Task<Result<byte[]>> SerializeMessagePackAsync<T>(T data, CompressionOption compressionOption) where T : notnull;

    /// <summary>
    ///     Deserializes data from MessagePack format with optional decompression.
    /// </summary>
    Task<Result<T>> DeserializeMessagePackAsync<T>(byte[]? data, CompressionOption compressionOption) where T : notnull;

    /// <summary>
    ///     Serializes data to MemoryPack format with optional compression.
    /// </summary>
    Task<Result<byte[]>> SerializeMemoryPackAsync<T>(T data, CompressionOption compressionOption) where T : notnull;

    /// <summary>
    ///     Deserializes data from MemoryPack format with optional decompression.
    /// </summary>
    Task<Result<T>> DeserializeMemoryPackAsync<T>(byte[]? data, CompressionOption compressionOption) where T : notnull;

    /// <summary>
    ///     Determines if a type is serializable by MessagePack by inspecting its attributes.
    ///     Results are cached to improve performance on subsequent checks.
    /// </summary>
    Task<Result> IsMessagePackSerializable<T>() where T : class;

    /// <summary>
    ///     Determines if a type is serializable by MemoryPack by inspecting its attributes.
    ///     Results are cached to improve performance on subsequent checks.
    /// </summary>
    Task<Result> IsMemoryPackSerializable<T>() where T : class;
}
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
    Task<Result<string>> SerializeJsonAsync<T>(T data, CompressionOption compressionOption,
        EncodingOption encodingOption);

    /// <summary>
    ///     Deserializes data from JSON format with optional compression and decoding.
    /// </summary>
    Task<Result<T>> DeserializeJsonAsync<T>(string data, CompressionOption compressionOption,
        EncodingOption encodingOption);

    /// <summary>
    ///     Serializes data to MessagePack format with optional compression.
    /// </summary>
    Task<Result<byte[]>> SerializeMessagePackAsync<T>(T data, CompressionOption compressionOption);

    /// <summary>
    ///     Deserializes data from MessagePack format with optional decompression.
    /// </summary>
    Task<Result<T>> DeserializeMessagePackAsync<T>(byte[] data, CompressionOption compressionOption);

    /// <summary>
    ///     Serializes data to MemoryPack format with optional compression.
    /// </summary>
    Task<Result<byte[]>> SerializeMemoryPackAsync<T>(T data, CompressionOption compressionOption);

    /// <summary>
    ///     Deserializes data from MemoryPack format with optional decompression.
    /// </summary>
    Task<Result<T>> DeserializeMemoryPackAsync<T>(byte[] data, CompressionOption compressionOption);

    /// <summary>
    ///     Determines if a type is serializable by MessagePack by inspecting its attributes.
    ///     Results are cached to improve performance on subsequent checks.
    /// </summary>
    Task<Result<bool>> IsMessagePackSerializable<T>() where T : class;
}
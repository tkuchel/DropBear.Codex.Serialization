using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;

namespace DropBear.Codex.Serialization.Interfaces;

/// <summary>
/// Provides methods for JSON serialization and deserialization with support for compression and encoding options.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Serializes an object to a JSON string with optional compression and encoding.
    /// </summary>
    Task<Result<string>?> SerializeAsync<T>(T data, CompressionOption compressionOption, EncodingOption encodingOption);

    /// <summary>
    /// Deserializes a JSON string to an object with optional decompression and decoding.
    /// </summary>
    Task<Result<T>?> DeserializeAsync<T>(string data, CompressionOption compressionOption, EncodingOption encodingOption) where T : notnull;
}
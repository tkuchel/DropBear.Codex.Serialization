using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;

namespace DropBear.Codex.Serialization.Interfaces;

/// <summary>
/// Defines methods for serializing and deserializing data using the MemoryPack format, with support for compression.
/// </summary>
public interface IMemoryPackSerializer
{
    /// <summary>
    /// Serializes an object to MemoryPack format with optional compression.
    /// </summary>
    Task<Result<byte[]>> SerializeAsync<T>(T? data, CompressionOption compressionOption);

    /// <summary>
    /// Deserializes data from MemoryPack format with optional decompression.
    /// </summary>
    Task<Result<T>> DeserializeAsync<T>(byte[]? data, CompressionOption compressionOption) where T : notnull;
}
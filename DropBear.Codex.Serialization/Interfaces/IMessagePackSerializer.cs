using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;

namespace DropBear.Codex.Serialization.Interfaces;

/// <summary>
/// Provides methods for serializing and deserializing data using the MessagePack format, with support for compression.
/// </summary>
public interface IMessagePackSerializer
{
    /// <summary>
    /// Serializes an object to MessagePack format with optional compression.
    /// </summary>
    Task<Result<byte[]>> SerializeAsync<T>(T data, CompressionOption compressionOption, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes data from MessagePack format with optional decompression.
    /// </summary>
    Task<Result<T>> DeserializeAsync<T>(byte[]? data, CompressionOption compressionOption, CancellationToken cancellationToken = default) where T : notnull;
}

using System.IO.Compression;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using MemoryPack;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Implements serialization and deserialization using MemoryPack with optional LZ4 compression.
/// </summary>
public class CustomMemoryPackSerializer : IMemoryPackSerializer
{
    private readonly ICompressionHelper _compressionHelper;
    private readonly ILogger<CustomMemoryPackSerializer> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomMemoryPackSerializer" /> class.
    /// </summary>
    /// <param name="logger">Logger for capturing runtime information and errors.</param>
    /// <param name="compressionHelper">Helper for managing data compression and decompression.</param>
    /// <exception cref="ArgumentNullException">Thrown if logger or compressionHelper is null.</exception>
    public CustomMemoryPackSerializer(ILogger<CustomMemoryPackSerializer> logger, ICompressionHelper compressionHelper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        _compressionHelper = compressionHelper ??
                             throw new ArgumentNullException(nameof(compressionHelper),
                                 "CompressionHelper cannot be null.");
    }

    /// <summary>
    ///     Serializes the given data to a byte array using MemoryPack, with optional LZ4 compression based on the specified
    ///     compression option.
    /// </summary>
    /// <typeparam name="T">The type of the data to serialize.</typeparam>
    /// <param name="data">The data to serialize. Must not be null.</param>
    /// <param name="compressionOption">Specifies whether to apply LZ4 compression.</param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous serialization operation, encapsulating the result as a
    ///     byte array.
    /// </returns>
    public async Task<Result<byte[]>> SerializeAsync<T>(T? data, CompressionOption compressionOption) where T : notnull
    {
        try
        {
            var bytes = MemoryPackSerializer.Serialize(data);
            if (compressionOption is CompressionOption.Compressed)
                bytes = await _compressionHelper.CompressAsync(bytes, CompressionType.Lz4, CompressionLevel.Optimal)
                    .ConfigureAwait(false);
            if (bytes is not null) return Result<byte[]>.Success(bytes);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"MemoryPack Serialization failed.");
            return Result<byte[]>.Failure("MemoryPack Serialization failed: " + ex.Message);
        }

        return Result<byte[]>.Failure("MemoryPack Serialization failed: Unknown error.");
    }

    /// <summary>
    ///     Deserializes the given byte array back into an instance of type T using MemoryPack, with optional LZ4 decompression
    ///     based on the specified compression option.
    /// </summary>
    /// <typeparam name="T">The type into which to deserialize the data.</typeparam>
    /// <param name="data">The byte array to deserialize. Must not be null or empty.</param>
    /// <param name="compressionOption">Specifies whether the data was compressed with LZ4 and should be decompressed.</param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous deserialization operation, encapsulating the
    ///     deserialized data.
    /// </returns>
    public async Task<Result<T>> DeserializeAsync<T>(byte[]? data, CompressionOption compressionOption) where T : notnull
    {
        try
        {
            if (compressionOption is CompressionOption.Compressed)
                data = await _compressionHelper.DecompressAsync(data, CompressionType.Lz4).ConfigureAwait(false);
            var result = MemoryPackSerializer.Deserialize<T>(data);
            if (result is not null) return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"MemoryPack Deserialization failed.");
            return Result<T>.Failure("MemoryPack Deserialization failed: " + ex.Message);
        }

        return Result<T>.Failure("MemoryPack Deserialization failed: Unknown error.");
    }
}

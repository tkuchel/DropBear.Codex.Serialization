using System.IO.Compression;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Implements serialization and deserialization using MemoryPack with optional LZ4 compression.
/// </summary>
public class MemoryPackSerializer : IMemoryPackSerializer
{
    private readonly ICompressionHelper _compressionHelper;
    private readonly ILogger<MemoryPackSerializer> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemoryPackSerializer" /> class.
    /// </summary>
    /// <param name="logger">Logger for capturing runtime information and errors.</param>
    /// <param name="compressionHelper">Helper for managing data compression and decompression.</param>
    /// <exception cref="ArgumentNullException">Thrown if logger or compressionHelper is null.</exception>
    public MemoryPackSerializer(ILogger<MemoryPackSerializer> logger, ICompressionHelper compressionHelper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        _compressionHelper = compressionHelper ??
                             throw new ArgumentNullException(nameof(compressionHelper),
                                 "CompressionHelper cannot be null.");
    }

    /// <inheritdoc />
    public async Task<Result<byte[]>> SerializeAsync<T>(T? data, CompressionOption compressionOption) where T : notnull
    {
        if (data == null)
        {
            _logger.LogWarning("SerializeAsync: Input data is null.");
            return Result<byte[]>.Failure("Input data is null.");
        }

        try
        {
            var bytes = MemoryPack.MemoryPackSerializer.Serialize(data);

            if (compressionOption == CompressionOption.Compressed)
                bytes = await _compressionHelper.CompressAsync(bytes, CompressionType.Lz4, CompressionLevel.Optimal);

            return Result<byte[]>.Success(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MemoryPack Serialization failed.");
            return Result<byte[]>.Failure($"MemoryPack Serialization failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<T>> DeserializeAsync<T>(byte[]? data, CompressionOption compressionOption)
        where T : notnull
    {
        if (data == null || data.Length == 0)
        {
            _logger.LogWarning("DeserializeAsync: Input data is null or empty.");
            return Result<T>.Failure("Input data is null or empty.");
        }

        try
        {
            if (compressionOption == CompressionOption.Compressed)
                data = await _compressionHelper.DecompressAsync(data, CompressionType.Lz4);

            var result = MemoryPack.MemoryPackSerializer.Deserialize<T>(data);
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MemoryPack Deserialization failed.");
            return Result<T>.Failure($"MemoryPack Deserialization failed: {ex.Message}");
        }
    }
}
using System.IO.Compression;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using ZLogger;

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
        if (data is null)
        {
            _logger.ZLogWarning($"SerializeAsync: Input data is null.");
            return Result<byte[]>.Failure("Input data is null.");
        }

        try
        {
            var bytes = MemoryPack.MemoryPackSerializer.Serialize(data);

            if (compressionOption is CompressionOption.Compressed)
                bytes = await _compressionHelper.CompressAsync(bytes, CompressionType.Lz4, CompressionLevel.Optimal).ConfigureAwait(false);

            if (bytes is not null) return Result<byte[]>.Success(bytes);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"MemoryPack Serialization failed.");
            return Result<byte[]>.Failure($"MemoryPack Serialization failed: {ex.Message}");
        }
        
        return Result<byte[]>.Failure("MemoryPack Serialization failed.");
    }

    /// <inheritdoc />
    public async Task<Result<T>> DeserializeAsync<T>(byte[]? data, CompressionOption compressionOption)
        where T : notnull
    {
        if (data is null || data.Length is 0)
        {
            _logger.ZLogWarning($"DeserializeAsync: Input data is null or empty.");
            return Result<T>.Failure("Input data is null or empty.");
        }

        try
        {
            if (compressionOption is CompressionOption.Compressed)
                data = await _compressionHelper.DecompressAsync(data, CompressionType.Lz4).ConfigureAwait(false);

            var result = MemoryPack.MemoryPackSerializer.Deserialize<T>(data);
            if (result is not null) return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex, $"MemoryPack Deserialization failed.");
            return Result<T>.Failure($"MemoryPack Deserialization failed: {ex.Message}");
        }
        
        return Result<T>.Failure("MemoryPack Deserialization failed.");
    }
}
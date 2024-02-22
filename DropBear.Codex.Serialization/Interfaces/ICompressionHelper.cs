using System.IO.Compression;
using DropBear.Codex.Serialization.Enums;

namespace DropBear.Codex.Serialization.Interfaces;

public interface ICompressionHelper
{
    /// <summary>
    ///     Asynchronously compresses data using the specified compression type and level.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <param name="compressionType">The compression algorithm to use.</param>
    /// <param name="compressionLevel">The compression level.</param>
    /// <returns>The compressed data.</returns>
    Task<byte[]?> CompressAsync(byte[]? data, CompressionType compressionType,
        CompressionLevel compressionLevel = CompressionLevel.Fastest);

    /// <summary>
    ///     Asynchronously decompresses data using the specified compression type.
    /// </summary>
    /// <param name="data">The data to decompress.</param>
    /// <param name="compressionType">The compression algorithm used during compression.</param>
    /// <returns>The decompressed data.</returns>
    Task<byte[]?> DecompressAsync(byte[]? data, CompressionType compressionType);
}
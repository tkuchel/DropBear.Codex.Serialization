using System.IO.Compression;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;

namespace DropBear.Codex.Serialization.Helpers;

/// <summary>
///     Provides methods for compressing and decompressing data using various compression algorithms.
/// </summary>
public class CompressionHelper : ICompressionHelper
{
    /// <summary>
    ///     Asynchronously compresses data using the specified compression type and level.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <param name="compressionType">The compression algorithm to use.</param>
    /// <param name="compressionLevel">The desired compression level.</param>
    /// <returns>A task that represents the asynchronous operation, resulting in the compressed data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported compression type is provided.</exception>
    public async Task<byte[]?> CompressAsync(byte[]? data, CompressionType compressionType,
        CompressionLevel compressionLevel = CompressionLevel.Fastest) =>
        compressionType switch
        {
            CompressionType.Brotli => await CompressWithBrotliAsync(data, compressionLevel).ConfigureAwait(false),
            CompressionType.Lz4 => await Task.FromResult(CompressWithLz4(data,
                compressionLevel)).ConfigureAwait(false), // Wrap synchronous method for API consistency.
            _ => throw new ArgumentOutOfRangeException(nameof(compressionType), "Unsupported compression type.")
        };

    /// <summary>
    ///     Asynchronously decompresses data using the specified compression type.
    /// </summary>
    /// <param name="data">The data to decompress.</param>
    /// <param name="compressionType">The compression algorithm used during compression.</param>
    /// <returns>A task that represents the asynchronous operation, resulting in the decompressed data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported compression type is provided.</exception>
    public async Task<byte[]?> DecompressAsync(byte[]? data, CompressionType compressionType) =>
        compressionType switch
        {
            CompressionType.Brotli => await DecompressWithBrotliAsync(data).ConfigureAwait(false),
            CompressionType.Lz4 => await Task.FromResult(
                DecompressWithLz4(data)).ConfigureAwait(false), // Wrap synchronous method for API consistency.
            _ => throw new ArgumentOutOfRangeException(nameof(compressionType), "Unsupported compression type.")
        };

    /// <summary>
    ///     Compresses data asynchronously using Brotli algorithm.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <param name="compressionLevel">The desired compression level.</param>
    /// <returns>A task that represents the asynchronous operation, containing the compressed data.</returns>
    private static async Task<byte[]?> CompressWithBrotliAsync(byte[]? data, CompressionLevel compressionLevel)
    {
        if (data is null) return null;

        using var output = new MemoryStream();
        var compressStream = new BrotliStream(output, compressionLevel, true);
        await using (compressStream.ConfigureAwait(false))
        {
            await compressStream.WriteAsync(data).ConfigureAwait(false);
        }

        return output.ToArray();
    }

    /// <summary>
    ///     Compresses data using LZ4 algorithm.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <param name="compressionLevel">The desired compression level.</param>
    /// <returns>The compressed data.</returns>
    private static byte[] CompressWithLz4(byte[]? data, CompressionLevel compressionLevel)
    {
        if (data is null) return Array.Empty<byte>();

        var settings = new LZ4EncoderSettings { CompressionLevel = MapCompressionLevel(compressionLevel) };
        using var output = new MemoryStream();
        using (var encoder = LZ4Stream.Encode(output, settings))
        {
            encoder.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    /// <summary>
    ///     Decompresses data asynchronously using Brotli algorithm.
    /// </summary>
    /// <param name="data">The data to decompress.</param>
    /// <returns>A task that represents the asynchronous operation, containing the decompressed data.</returns>
    private static async Task<byte[]?> DecompressWithBrotliAsync(byte[]? data)
    {
        if (data is null) return null;

        using var input = new MemoryStream(data);
        using var output = new MemoryStream();
        var decompressStream = new BrotliStream(input, CompressionMode.Decompress);
        await using (decompressStream.ConfigureAwait(false))
        {
            await decompressStream.CopyToAsync(output).ConfigureAwait(false);
        }

        return output.ToArray();
    }

    /// <summary>
    ///     Decompresses data using LZ4 algorithm.
    /// </summary>
    /// <param name="data">The data to decompress.</param>
    /// <returns>The decompressed data.</returns>
    private static byte[] DecompressWithLz4(byte[]? data)
    {
        if (data is null) return Array.Empty<byte>();

        using var input = new MemoryStream(data);
        using var output = new MemoryStream();
        using (var decoder = LZ4Stream.Decode(input))
        {
            decoder.CopyTo(output);
        }

        return output.ToArray();
    }

    /// <summary>
    ///     Maps a generic compression level to LZ4's specific compression levels.
    /// </summary>
    /// <param name="compressionLevel">The generic compression level.</param>
    /// <returns>The LZ4 specific compression level.</returns>
    private static LZ4Level MapCompressionLevel(CompressionLevel compressionLevel) =>
        compressionLevel switch
        {
            CompressionLevel.Optimal => LZ4Level.L12_MAX, // Maps to the maximum compression level in LZ4.
            CompressionLevel.Fastest => LZ4Level.L00_FAST, // Maps to the fastest compression level in LZ4.
            _ => LZ4Level.L03_HC // Default case uses a moderate compression level.
        };
}

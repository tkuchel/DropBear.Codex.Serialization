using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Helpers;

using System.IO.Compression;

using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;


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
    /// <param name="compressionLevel">The compression level.</param>
    /// <returns>The compressed data.</returns>
    public async Task<byte[]?> CompressAsync(byte[]? data, CompressionType compressionType,
        CompressionLevel compressionLevel = CompressionLevel.Fastest)
    {
        return compressionType switch
        {
            CompressionType.Brotli => await CompressWithBrotliAsync(data, compressionLevel),
            CompressionType.LZ4 => CompressWithLZ4(data, compressionLevel),
            _ => throw new ArgumentOutOfRangeException(nameof(compressionType), "Unsupported compression type.")
        };
    }

    /// <summary>
    ///     Asynchronously decompresses data using the specified compression type.
    /// </summary>
    /// <param name="data">The data to decompress.</param>
    /// <param name="compressionType">The compression algorithm used during compression.</param>
    /// <returns>The decompressed data.</returns>
    public async Task<byte[]?> DecompressAsync(byte[]? data, CompressionType compressionType)
    {
        return compressionType switch
        {
            CompressionType.Brotli => await DecompressWithBrotliAsync(data),
            CompressionType.LZ4 => DecompressWithLZ4(data),
            _ => throw new ArgumentOutOfRangeException(nameof(compressionType), "Unsupported compression type.")
        };
    }

    private async Task<byte[]?> CompressWithBrotliAsync(byte[]? data, CompressionLevel compressionLevel)
    {
        await using var output = new MemoryStream();
        // Use BrotliStream for compression. The compression level is cast from the System.IO.Compression enum to match Brotli's expectations.
        await using (var compressStream = new BrotliStream(output, compressionLevel, true))
        {
            await compressStream.WriteAsync(data, 0, data.Length);
        }

        return output.ToArray();
    }

    private byte[]? CompressWithLZ4(byte[]? data, CompressionLevel compressionLevel)
    {
        var settings = new LZ4EncoderSettings
        {
            CompressionLevel =
                MapCompressionLevel(compressionLevel) // Map the generic compression level to LZ4's specific level.
        };

        using var output = new MemoryStream();
        using (var encoder = LZ4Stream.Encode(output, settings))
        {
            encoder.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    private async Task<byte[]?> DecompressWithBrotliAsync(byte[]? data)
    {
        await using var input = new MemoryStream(data);
        await using var output = new MemoryStream();
        await using (var decompressStream = new BrotliStream(input, CompressionMode.Decompress))
        {
            await decompressStream.CopyToAsync(output);
        }

        return output.ToArray();
    }

    private byte[]? DecompressWithLZ4(byte[]? data)
    {
        using var input = new MemoryStream(data);
        using var output = new MemoryStream();
        using (var decoder = LZ4Stream.Decode(input))
        {
            decoder.CopyTo(output);
        }

        return output.ToArray();
    }

    private LZ4Level MapCompressionLevel(CompressionLevel compressionLevel)
    {
        // Maps the generic CompressionLevel to LZ4's specific compression levels.
        return compressionLevel switch
        {
            CompressionLevel.Optimal => LZ4Level.L12_MAX, // Maps to the maximum compression level in LZ4.
            CompressionLevel.Fastest => LZ4Level.L00_FAST, // Maps to the fastest compression level in LZ4.
            _ => LZ4Level.L03_HC // Default case uses a moderate compression level.
        };
    }
}


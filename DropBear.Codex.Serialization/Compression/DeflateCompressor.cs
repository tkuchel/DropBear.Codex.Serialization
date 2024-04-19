using System.IO.Compression;
using DropBear.Codex.Serialization.Exceptions;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Compression;

/// <summary>
///     Provides methods to compress and decompress data using the Deflate algorithm.
/// </summary>
public class DeflateCompressor : ICompressor
{
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeflateCompressor" /> class.
    /// </summary>
    public DeflateCompressor() => _memoryStreamManager = new RecyclableMemoryStreamManager();
#pragma warning disable MA0004 // Use ConfigureAwait
    /// <inheritdoc />
    public async Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data), "Input data cannot be null.");

        await using var compressedStream = _memoryStreamManager.GetStream("DeflateCompressor-Compress");
        try
        {
            await using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress, true);
            await deflateStream.WriteAsync(data.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new CompressionException("Error occurred while compressing data.", ex);
        }

        compressedStream.Position = 0; // Reset position to read the stream content
        return compressedStream.ToArray();
    }

    /// <inheritdoc />
    public async Task<byte[]> DecompressAsync(byte[] compressedData, CancellationToken cancellationToken = default)
    {
        if (compressedData is null)
            throw new ArgumentNullException(nameof(compressedData), "Compressed data cannot be null.");

        await using var compressedStream = _memoryStreamManager.GetStream("DeflateCompressor-Decompress-Input", compressedData);
        await using var decompressedStream = _memoryStreamManager.GetStream("DeflateCompressor-Decompress-Output");

        try
        {
            await using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
            await deflateStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new CompressionException("Error occurred while decompressing data.", ex);
        }

        decompressedStream.Position = 0; // Reset position to read the stream content
        return decompressedStream.ToArray();
    }
#pragma warning restore MA0004 // Use ConfigureAwait
}

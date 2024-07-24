#region

using System.IO.Compression;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

#endregion

namespace DropBear.Codex.Serialization.Compression;

/// <summary>
///     Provides methods to compress and decompress data using the GZip algorithm.
/// </summary>
public class GZipCompressor : ICompressor
{
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GZipCompressor" /> class.
    /// </summary>
    public GZipCompressor()
    {
        _memoryStreamManager = new RecyclableMemoryStreamManager();
    }
#pragma warning disable MA0004
    /// <inheritdoc />
    public async Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data), "Input data cannot be null.");
        await using var compressedStream = _memoryStreamManager.GetStream("GZipCompressor-Compress");
        await using (compressedStream)
        {
            await using var
                zipStream = new GZipStream(compressedStream, CompressionMode.Compress, false); // Set leaveOpen to false
            await zipStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            await zipStream.FlushAsync(cancellationToken); // Flush the stream to ensure all data is written
            compressedStream.Position = 0; // Reset position to read the stream content
            return compressedStream.ToArray();
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> DecompressAsync(byte[] compressedData, CancellationToken cancellationToken = default)
    {
        _ = compressedData ??
            throw new ArgumentNullException(nameof(compressedData), "Compressed data cannot be null.");

        await using var compressedStream =
            _memoryStreamManager.GetStream("GZipCompressor-Decompress-Input", compressedData);
        await using var decompressedStream = _memoryStreamManager.GetStream("GZipCompressor-Decompress-Output");

        await using (compressedStream)
        await using (decompressedStream)
        {
            await using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            await zipStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);
            decompressedStream.Position = 0; // Reset position to read the stream content
            return decompressedStream.ToArray();
        }
    }
#pragma warning restore MA0004
}

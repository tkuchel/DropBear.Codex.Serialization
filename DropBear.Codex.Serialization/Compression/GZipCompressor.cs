using System.IO.Compression;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Compression;

public class GZipCompressor : ICompressor
{
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    public GZipCompressor() => _memoryStreamManager = new RecyclableMemoryStreamManager();

    public Task<byte[]> CompressAsync(byte[] data, CancellationToken token)
    {
        using var compressedStream = _memoryStreamManager.GetStream("GZipCompressor-Compress");
        using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress, true);
        zipStream.Write(data, 0, data.Length);
        zipStream.Close();
        compressedStream.Position = 0; // Reset position to read the stream content
        return compressedStream.ToArray();
    }

    public Task<byte[]> DecompressAsync(byte[] data, CancellationToken token)
    {
        using var compressedStream = _memoryStreamManager.GetStream("GZipCompressor-Decompress-Input", data);
        using var decompressedStream = _memoryStreamManager.GetStream("GZipCompressor-Decompress-Output");
        using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        zipStream.CopyTo(decompressedStream);
        decompressedStream.Position = 0; // Reset position to read the stream content
        return decompressedStream.ToArray();
    }
}

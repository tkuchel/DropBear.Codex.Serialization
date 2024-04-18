namespace DropBear.Codex.Serialization.Interfaces;

public interface ICompressor
{
    Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken = default);
    Task<byte[]> DecompressAsync(byte[] compressedData, CancellationToken cancellationToken = default);
}

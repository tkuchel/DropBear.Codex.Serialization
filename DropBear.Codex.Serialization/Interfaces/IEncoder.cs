namespace DropBear.Codex.Serialization.Interfaces;

public interface IEncoder
{
    Task<byte[]> EncodeAsync(byte[] data, CancellationToken cancellationToken = default);
    Task<byte[]> DecodeAsync(byte[] encodedData, CancellationToken cancellationToken = default);
}

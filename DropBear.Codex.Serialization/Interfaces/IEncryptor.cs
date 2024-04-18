namespace DropBear.Codex.Serialization.Interfaces;

public interface IEncryptor
{
    Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken = default);
    Task<byte[]> DecryptAsync(byte[] encryptedData, CancellationToken cancellationToken = default);
}

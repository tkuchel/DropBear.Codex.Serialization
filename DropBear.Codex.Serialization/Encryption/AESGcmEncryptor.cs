using System.Security.Cryptography;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Encryption;

/// <summary>
///     Provides methods to encrypt and decrypt data using AES-GCM encryption.
/// </summary>
public class AesGcmEncryptor : IEncryptor, IDisposable
{
    private const int KeySize = 32; // AES-256 key size in bytes

    private const int Tagsize = 16;
    private readonly RSA _rsa;
    private byte[] _key;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AesGcmEncryptor" /> class with the specified RSA key pair.
    /// </summary>
    /// <param name="rsa">The RSA key pair used for encryption.</param>
    public AesGcmEncryptor(RSA rsa)
    {
        _rsa = rsa ?? throw new ArgumentNullException(nameof(rsa), "RSA key pair cannot be null.");
        _key = GenerateKey();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Array.Clear(_key, 0, _key.Length); // Securely erase the key
        _rsa.Dispose();
    }

    /// <inheritdoc />
    public Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data), "Input data cannot be null.");

        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);
        var ciphertext = new byte[data.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aesGcm = new AesGcm(_key, Tagsize);
        aesGcm.Encrypt(nonce, data, ciphertext, tag);

        var encryptedKey = _rsa.Encrypt(_key, RSAEncryptionPadding.OaepSHA256);
        var encryptedNonce = _rsa.Encrypt(nonce, RSAEncryptionPadding.OaepSHA256);

        var combinedData = Combine(encryptedKey, encryptedNonce, tag, ciphertext);
        return Task.FromResult(combinedData);
    }

    /// <inheritdoc />
    public Task<byte[]> DecryptAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        _ = data ??
            throw new ArgumentNullException(nameof(data), "Input encrypted data cannot be null.");

        var keySizeBytes = _rsa.KeySize / 8;
        var encryptedKey = data.Take(keySizeBytes).ToArray();
        var encryptedNonce = data.Skip(keySizeBytes).Take(keySizeBytes).ToArray();
        var tag = data.Skip(2 * keySizeBytes).Take(AesGcm.TagByteSizes.MaxSize).ToArray();
        var ciphertext = data.Skip(2 * keySizeBytes + tag.Length).ToArray();

        _key = _rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
        var nonce = _rsa.Decrypt(encryptedNonce, RSAEncryptionPadding.OaepSHA256);

        var plaintext = new byte[ciphertext.Length];
        using var aesGcm = new AesGcm(_key, Tagsize);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return Task.FromResult(plaintext);
    }

    private static byte[] GenerateKey()
    {
        var key = new byte[KeySize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return key;
    }

    private static byte[] Combine(params byte[][] arrays)
    {
        var combined = new byte[arrays.Sum(a => a.Length)];
        var offset = 0;
        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, combined, offset, array.Length);
            offset += array.Length;
        }

        return combined;
    }
}

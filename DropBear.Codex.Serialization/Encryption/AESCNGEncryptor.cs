using System.Runtime.Versioning;
using System.Security.Cryptography;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Encryption;

/// <summary>
///     Provides methods to encrypt and decrypt data using AES encryption.
/// </summary>
public class AESCNGEncryptor : IEncryptor, IDisposable
{
    private readonly AesCng _aesCng;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;
    private readonly RSA _rsa;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AESCNGEncryptor" /> class with the specified RSA key pair.
    /// </summary>
    /// <param name="rsa">The RSA key pair used for encryption.</param>
    /// <param name="memoryStreamManager">The RecyclableMemoryStreamManager instance</param>
    [SupportedOSPlatform("windows")]
    public AESCNGEncryptor(RSA rsa, RecyclableMemoryStreamManager memoryStreamManager)
    {
        _aesCng = new AesCng();
        _rsa = rsa ?? throw new ArgumentNullException(nameof(rsa), "RSA key pair cannot be null.");
        _memoryStreamManager = memoryStreamManager ?? throw new ArgumentNullException(nameof(memoryStreamManager),
            "Memory stream manager cannot be null.");
        InitializeCryptoComponents();
    }

    /// <inheritdoc />
    public void Dispose() => _aesCng.Dispose();
#pragma warning disable MA0004
    /// <inheritdoc />
    public async Task<byte[]> EncryptAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data), "Input data cannot be null.");

        var encryptedKey = _rsa.Encrypt(_aesCng.Key, RSAEncryptionPadding.OaepSHA256);
        var encryptedIV = _rsa.Encrypt(_aesCng.IV, RSAEncryptionPadding.OaepSHA256);

        using var encryptor = _aesCng.CreateEncryptor();
        await using var resultStream = _memoryStreamManager.GetStream("AesEncryptor-Encrypt");
        await using var cryptoStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write);
        await cryptoStream.WriteAsync(data.AsMemory(), cancellationToken).ConfigureAwait(false);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken).ConfigureAwait(false);
        return Combine(encryptedKey, encryptedIV, resultStream.ToArray());
    }

    /// <inheritdoc />
    public async Task<byte[]> DecryptAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data), "Input data cannot be null.");

        // Extract encrypted key, IV, and data
        var keySizeBytes = _rsa.KeySize / 8; // Calculate based on RSA key size
        var encryptedKey = data.Take(keySizeBytes).ToArray();
        var encryptedIV = data.Skip(keySizeBytes).Take(keySizeBytes).ToArray();
        var encryptedData = data.Skip(2 * keySizeBytes).ToArray();

        // Decrypt key and IV
        byte[] aesKey;
        byte[] aesIV;
        try
        {
            aesKey = _rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
            aesIV = _rsa.Decrypt(encryptedIV, RSAEncryptionPadding.OaepSHA256);
        }
        catch (CryptographicException e)
        {
            throw new InvalidOperationException("Decryption failed. RSA key or padding is incorrect.", e);
        }

        // Decrypt data
        using var decryptor = _aesCng.CreateDecryptor(aesKey, aesIV);
        await using var resultStream = _memoryStreamManager.GetStream("AesEncryptor-Decrypt");
        await using var cryptoStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write);
        await cryptoStream.WriteAsync(encryptedData.AsMemory(), cancellationToken).ConfigureAwait(false);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken).ConfigureAwait(false);
        // Read the decrypted data from the result stream
        resultStream.Position = 0; // Reset the stream position to the beginning before reading
        var decryptedData = new byte[resultStream.Length];
        int bytesReadTotal = 0, bytesRead = 0;
        while ((bytesRead = await resultStream
                   .ReadAsync(decryptedData.AsMemory(bytesReadTotal, decryptedData.Length - bytesReadTotal),
                       cancellationToken).ConfigureAwait(false)) > 0) bytesReadTotal += bytesRead;

        return decryptedData.AsSpan(0, bytesReadTotal).ToArray(); // Only return the actual data read
    }
#pragma warning restore MA0004
    private void InitializeCryptoComponents()
    {
        using var rng = RandomNumberGenerator.Create();
        _aesCng.Key = new byte[32]; // 256 bits for AES-256
        _aesCng.IV = new byte[16]; // 128 bits for AES block size
        rng.GetBytes(_aesCng.Key);
        rng.GetBytes(_aesCng.IV);
    }

    private static byte[] Combine(params byte[][] arrays)
    {
        var totalLength = arrays.Sum(array => array.Length);
        var result = new byte[totalLength];
        var offset = 0;
        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
    }
}

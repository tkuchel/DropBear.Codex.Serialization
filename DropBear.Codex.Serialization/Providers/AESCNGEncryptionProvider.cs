using System.Runtime.Versioning;
using System.Security.Cryptography;
using DropBear.Codex.Serialization.Encryption;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Providers;

/// <summary>
///     Provides AES encryption services.
/// </summary>
public class AESCNGEncryptionProvider : IEncryptionProvider
{
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;
    private readonly RSA _rsa;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AESCNGEncryptionProvider" /> class.
    /// </summary>
    /// <param name="recyclableMemoryStreamManager">The stream manager instance to use.</param>
    /// <param name="publicKeyPath">The path to the public key file.</param>
    /// <param name="privateKeyPath">The path to the private key file.</param>
    [SupportedOSPlatform("windows")]
    public AESCNGEncryptionProvider(RecyclableMemoryStreamManager recyclableMemoryStreamManager, string publicKeyPath,
        string privateKeyPath)
    {
        var rsaKeyProvider = new RSAKeyProvider(publicKeyPath, privateKeyPath);
        _rsa = rsaKeyProvider.GetRsaProvider();
        _memoryStreamManager = recyclableMemoryStreamManager;
    }

    /// <summary>
    ///     Gets an AES encryptor.
    /// </summary>
    /// <returns>An AES encryptor.</returns>
    [SupportedOSPlatform("windows")]
    public IEncryptor GetEncryptor() => new AESCNGEncryptor(_rsa, _memoryStreamManager);
}

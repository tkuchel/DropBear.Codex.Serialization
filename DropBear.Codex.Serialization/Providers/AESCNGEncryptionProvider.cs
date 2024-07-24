#region

using System.Runtime.Versioning;
using System.Security.Cryptography;
using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Encryption;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.IO;

#endregion

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
    [SupportedOSPlatform("windows")]
    public AESCNGEncryptionProvider(SerializationConfig config,
        RecyclableMemoryStreamManager recyclableMemoryStreamManager)
    {
        var rsaKeyProvider = new RSAKeyProvider(config.PublicKeyPath, config.PrivateKeyPath);
        _rsa = rsaKeyProvider.GetRsaProvider();
        _memoryStreamManager = recyclableMemoryStreamManager;
    }

    /// <summary>
    ///     Gets an AES encryptor.
    /// </summary>
    /// <returns>An AES encryptor.</returns>
    [SupportedOSPlatform("windows")]
    public IEncryptor GetEncryptor()
    {
        return new AESCNGEncryptor(_rsa, _memoryStreamManager);
    }
}

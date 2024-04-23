using System.Runtime.Versioning;
using System.Security.Cryptography;
using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Encryption;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers;

/// <summary>
///     Provides RSA encryption services.
/// </summary>
public class AESGCMEncryptionProvider : IEncryptionProvider
{
    private readonly RSA _rsa;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AESGCMEncryptionProvider" /> class with the specified paths to public
    ///     and private keys.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public AESGCMEncryptionProvider(SerializationConfig config)
    {
        var rsaKeyProvider = new RSAKeyProvider(config.PublicKeyPath, config.PrivateKeyPath);
        _rsa = rsaKeyProvider.GetRsaProvider();
    }

    /// <summary>
    ///     Gets an AES-GCM encryptor using RSA encryption.
    /// </summary>
    /// <returns>An AES-GCM encryptor using RSA encryption.</returns>
    public IEncryptor GetEncryptor() => new AesGcmEncryptor(_rsa);
}

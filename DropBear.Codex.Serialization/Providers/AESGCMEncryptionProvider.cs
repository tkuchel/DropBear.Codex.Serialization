using System.Runtime.Versioning;
using System.Security.Cryptography;
using DropBear.Codex.Serialization.Encryption;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers
{
    /// <summary>
    /// Provides RSA encryption services.
    /// </summary>
    public class AESGCMEncryptionProvider : IEncryptionProvider
    {
        private readonly RSA _rsa;

        /// <summary>
        /// Initializes a new instance of the <see cref="AESGCMEncryptionProvider"/> class with the specified paths to public and private keys.
        /// </summary>
        /// <param name="publicKeyPath">The path to the public key file.</param>
        /// <param name="privateKeyPath">The path to the private key file.</param>
        [SupportedOSPlatform("windows")]
        public AESGCMEncryptionProvider(string publicKeyPath, string privateKeyPath)
        {
            var rsaKeyProvider = new RSAKeyProvider(publicKeyPath, privateKeyPath);
            _rsa = rsaKeyProvider.GetRsaProvider();
        }

        /// <summary>
        /// Gets an AES-GCM encryptor using RSA encryption.
        /// </summary>
        /// <returns>An AES-GCM encryptor using RSA encryption.</returns>
        public IEncryptor GetEncryptor() => new AesGcmEncryptor(_rsa);
    }
}

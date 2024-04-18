using DropBear.Codex.Serialization.Encryption;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers;

public class AESEncryptionProvider : IEncryptionProvider
{
    public IEncryptor GetEncryptor() => new AesEncryptor();
}

using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers;

public class RSAEncryptionProvider : IEncryptionProvider
{
    public IEncryptor GetEncryptor() => new RSAEncryptor();
}

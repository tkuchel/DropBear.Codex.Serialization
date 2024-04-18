namespace DropBear.Codex.Serialization.Interfaces;

public interface IEncryptionProvider
{
    IEncryptor GetEncryptor();
}

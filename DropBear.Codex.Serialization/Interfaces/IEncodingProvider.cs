namespace DropBear.Codex.Serialization.Interfaces;

public interface IEncodingProvider
{
    IEncoder GetEncoder();
}

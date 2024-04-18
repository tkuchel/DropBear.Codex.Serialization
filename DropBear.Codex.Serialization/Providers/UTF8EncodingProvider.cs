using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers;

public class UTF8EncodingProvider : IEncodingProvider
{
    public IEncoder GetEncoder() => new UTF8Encoder();
}

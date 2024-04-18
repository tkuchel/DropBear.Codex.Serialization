using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers;

public class GZipCompressionProvider : ICompressionProvider
{
    public ICompressor GetCompressor() => new GZipCompressor();
}

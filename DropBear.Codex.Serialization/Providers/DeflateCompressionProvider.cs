using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers;

public class DeflateCompressionProvider : ICompressionProvider
{
    public ICompressor GetCompressor() => new DeflateCompressor();
}

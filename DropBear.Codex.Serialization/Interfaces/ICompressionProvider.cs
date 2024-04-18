namespace DropBear.Codex.Serialization.Interfaces;

public interface ICompressionProvider
{
    ICompressor GetCompressor();
}

#region

using DropBear.Codex.Serialization.Compression;
using DropBear.Codex.Serialization.Interfaces;

#endregion

namespace DropBear.Codex.Serialization.Providers;

/// <summary>
///     Provides Deflate compression services.
/// </summary>
public class DeflateCompressionProvider : ICompressionProvider
{
    /// <summary>
    ///     Gets a Deflate compressor.
    /// </summary>
    /// <returns>A Deflate compressor.</returns>
    public ICompressor GetCompressor()
    {
        return new DeflateCompressor();
    }
}

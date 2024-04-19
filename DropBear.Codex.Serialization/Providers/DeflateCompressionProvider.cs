using DropBear.Codex.Serialization.Compression;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers
{
    /// <summary>
    /// Provides Deflate compression services.
    /// </summary>
    public class DeflateCompressionProvider : ICompressionProvider
    {
        /// <summary>
        /// Gets a Deflate compressor.
        /// </summary>
        /// <returns>A Deflate compressor.</returns>
        public ICompressor GetCompressor() => new DeflateCompressor();
    }
}

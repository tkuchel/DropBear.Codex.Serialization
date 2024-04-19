﻿using DropBear.Codex.Serialization.Compression;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers
{
    /// <summary>
    /// Provides GZip compression services.
    /// </summary>
    public class GZipCompressionProvider : ICompressionProvider
    {
        /// <summary>
        /// Gets a GZip compressor.
        /// </summary>
        /// <returns>A GZip compressor.</returns>
        public ICompressor GetCompressor() => new GZipCompressor();
    }
}

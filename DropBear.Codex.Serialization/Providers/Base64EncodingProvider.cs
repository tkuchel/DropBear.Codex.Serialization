using DropBear.Codex.Serialization.Encoders;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Providers
{
    /// <summary>
    /// Provides Base64 encoding services.
    /// </summary>
    public class Base64EncodingProvider : IEncodingProvider
    {
        /// <summary>
        /// Gets a Base64 encoder.
        /// </summary>
        /// <returns>A Base64 encoder.</returns>
        public IEncoder GetEncoder() => new Base64Encoder();
    }
}

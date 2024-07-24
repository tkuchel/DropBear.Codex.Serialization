#region

using DropBear.Codex.Serialization.Encoders;
using DropBear.Codex.Serialization.Interfaces;

#endregion

namespace DropBear.Codex.Serialization.Providers;

/// <summary>
///     Provides hexadecimal encoding services.
/// </summary>
public class HexEncodingProvider : IEncodingProvider
{
    /// <summary>
    ///     Gets a hexadecimal encoder.
    /// </summary>
    /// <returns>A hexadecimal encoder.</returns>
    public IEncoder GetEncoder()
    {
        return new HexEncoder();
    }
}

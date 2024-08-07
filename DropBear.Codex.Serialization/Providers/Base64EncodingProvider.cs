﻿#region

using DropBear.Codex.Serialization.Encoders;
using DropBear.Codex.Serialization.Interfaces;

#endregion

namespace DropBear.Codex.Serialization.Providers;

/// <summary>
///     Provides Base64 encoding services.
/// </summary>
public class Base64EncodingProvider : IEncodingProvider
{
    /// <summary>
    ///     Gets a Base64 encoder.
    /// </summary>
    /// <returns>A Base64 encoder.</returns>
    public IEncoder GetEncoder()
    {
        return new Base64Encoder();
    }
}

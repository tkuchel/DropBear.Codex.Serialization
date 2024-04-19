using System.Text;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Encoders;

/// <summary>
///     Provides methods to encode and decode data using Base64 encoding.
/// </summary>
public class Base64Encoder : IEncoder
{
    /// <inheritdoc />
    public Task<byte[]> EncodeAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data), "Input data cannot be null.");

        return Task.FromResult(Encoding.UTF8.GetBytes(Convert.ToBase64String(data)));
    }

    /// <inheritdoc />
    public Task<byte[]> DecodeAsync(byte[] encodedData, CancellationToken cancellationToken = default)
    {
        _ = encodedData ?? throw new ArgumentNullException(nameof(encodedData), "Encoded data cannot be null.");

        var base64EncodedString = Encoding.UTF8.GetString(encodedData);
        return Task.FromResult(Convert.FromBase64String(base64EncodedString));
    }
}

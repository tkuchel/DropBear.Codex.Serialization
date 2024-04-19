using System.Text;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Encoders;

/// <summary>
///     Provides methods to encode and decode data using hexadecimal encoding.
/// </summary>
public class HexEncoder : IEncoder
{
    /// <inheritdoc />
    public Task<byte[]> EncodeAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data), "Input data cannot be null.");

        var hexString = BitConverter.ToString(data).Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(Encoding.UTF8.GetBytes(hexString));
    }

    /// <inheritdoc />
    public Task<byte[]> DecodeAsync(byte[] encodedData, CancellationToken cancellationToken = default)
    {
        _ = encodedData ?? throw new ArgumentNullException(nameof(encodedData), "Encoded data cannot be null.");

        var hexString = Encoding.UTF8.GetString(encodedData);
        return Task.FromResult(ConvertHexStringToByteArray(hexString));
    }

    private static byte[] ConvertHexStringToByteArray(string hexString)
    {
        _ = hexString ?? throw new ArgumentNullException(nameof(hexString), "Hexadecimal string cannot be null.");

        if (hexString.Length % 2 is not 0)
            throw new ArgumentException("Hexadecimal string must have an even length", nameof(hexString));

        var bytes = new byte[hexString.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            var currentHex = hexString.Substring(i * 2, 2);
            bytes[i] = Convert.ToByte(currentHex, 16);
        }

        return bytes;
    }
}

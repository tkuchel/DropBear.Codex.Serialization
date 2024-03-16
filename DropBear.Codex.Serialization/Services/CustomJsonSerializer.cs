using System.Text;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using ServiceStack.Text;
using ZLogger;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Provides JSON serialization and deserialization services with optional compression and encoding.
/// </summary>
public class CustomJsonSerializer : IJsonSerializer
{
    private readonly ICompressionHelper _compressionHelper;
    private readonly ILogger<CustomJsonSerializer> _logger;

    /// <summary>
    ///     Constructs a JsonSerializer with logging and compression capabilities.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="compressionHelper">Compression helper for handling data compression and decompression.</param>
    public CustomJsonSerializer(ILogger<CustomJsonSerializer> logger, ICompressionHelper compressionHelper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        _compressionHelper = compressionHelper ??
                             throw new ArgumentNullException(nameof(compressionHelper),
                                 "Compression helper cannot be null.");
    }

    /// <inheritdoc />
    public async Task<Result<string>?> SerializeAsync<T>(T data, CompressionOption compressionOption,
        EncodingOption encodingOption)
    {
        if (data == null)
            return LogAndReturnFailure<string>("SerializeAsync: Input data is null.");

        try
        {
            var jsonData = JsonSerializer.SerializeToString(data);
            var bytes = Encoding.UTF8.GetBytes(jsonData);
            bytes = await HandleCompressionAsync(bytes, compressionOption).ConfigureAwait(false);
            return Result<string>.Success(HandleEncoding(bytes, encodingOption));
        }
        catch (Exception ex)
        {
            return LogAndReturnFailure<string>($"JSON Serialization failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<T>?> DeserializeAsync<T>(string data, CompressionOption compressionOption,
        EncodingOption encodingOption) where T : notnull
    {
        if (string.IsNullOrWhiteSpace(data))
            return LogAndReturnFailure<T>("DeserializeAsync: Input string is null or whitespace.");

        try
        {
            var bytes = HandleDecoding(data, encodingOption);
            bytes = await HandleCompressionAsync(bytes, compressionOption, false).ConfigureAwait(false);

            if (bytes is not null)
            {
                var jsonData = Encoding.UTF8.GetString(bytes);
                var result = JsonSerializer.DeserializeFromString<T>(jsonData);
                if (result is not null) return Result<T>.Success(result);
            }
            else
            {
                return LogAndReturnFailure<T>("DeserializeAsync: Decoded bytes are null.");
            }
        }
        catch (Exception ex)
        {
            return LogAndReturnFailure<T>($"JSON Deserialization failed: {ex.Message}");
        }

        return LogAndReturnFailure<T>("DeserializeAsync: Deserialization failed.");
    }

    /// <summary>
    ///     Handles compression asynchronously based on the specified compression option.
    /// </summary>
    /// <param name="data">The data to compress or decompress.</param>
    /// <param name="option">The compression option.</param>
    /// <param name="compress">Determines whether to compress or decompress the data.</param>
    /// <returns>A task that represents the asynchronous operation, containing the processed data.</returns>
    private async Task<byte[]?> HandleCompressionAsync(byte[]? data, CompressionOption option, bool compress = true) =>
        option switch
        {
            CompressionOption.Compressed => compress
                ? await _compressionHelper.CompressAsync(data, CompressionType.Brotli)
                    .ConfigureAwait(false)
                : await _compressionHelper.DecompressAsync(data, CompressionType.Brotli).ConfigureAwait(false),
            CompressionOption.None => data,
            _ => throw new ArgumentException("Invalid compression option.", nameof(option))
        };

    /// <summary>
    ///     Handles encoding of byte data to a string based on the specified encoding option.
    /// </summary>
    /// <param name="data">The byte array to encode.</param>
    /// <param name="option">The encoding option.</param>
    /// <returns>The encoded string.</returns>
    private static string HandleEncoding(byte[]? data, EncodingOption option)
    {
        if (data is not null)
            return option switch
            {
                EncodingOption.Base64 => Convert.ToBase64String(data),
                EncodingOption.Plain => Encoding.UTF8.GetString(data),
                _ => throw new ArgumentException("Invalid encoding option.", nameof(option))
            };

        throw new ArgumentNullException(nameof(data), "Data cannot be null.");
    }

    /// <summary>
    ///     Handles decoding of a string to byte data based on the specified encoding option.
    /// </summary>
    /// <param name="data">The string to decode.</param>
    /// <param name="option">The encoding option.</param>
    /// <returns>The decoded byte array.</returns>
    private static byte[] HandleDecoding(string data, EncodingOption option) =>
        option switch
        {
            EncodingOption.Base64 => Convert.FromBase64String(data),
            EncodingOption.Plain => Encoding.UTF8.GetBytes(data),
            _ => throw new ArgumentException("Invalid encoding option.", nameof(option))
        };

    private Result<T>? LogAndReturnFailure<T>(string? message) where T : notnull
    {
        message ??= "Unknown error";
        _logger.ZLogError($"{message}");
        return Result<T>.Failure(message);
    }
}

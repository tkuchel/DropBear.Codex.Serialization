using System.Text;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Provides JSON serialization and deserialization services with optional compression and encoding.
/// </summary>
public class JsonSerializer : IJsonSerializer
{
    private readonly ICompressionHelper _compressionHelper;
    private readonly ILogger<JsonSerializer> _logger;

    /// <summary>
    ///     Constructs a JsonSerializer with logging and compression capabilities.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="compressionHelper">Compression helper for handling data compression and decompression.</param>
    public JsonSerializer(ILogger<JsonSerializer> logger, ICompressionHelper compressionHelper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
        _compressionHelper = compressionHelper ??
                             throw new ArgumentNullException(nameof(compressionHelper),
                                 "Compression helper cannot be null.");
    }

    /// <inheritdoc />
    public async Task<Result<string>> SerializeAsync<T>(T data, CompressionOption compressionOption,
        EncodingOption encodingOption)
    {
        if (data == null)
            return LogAndReturnFailure<string>("SerializeJsonAsync: Input data is null.");

        try
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var bytes = Encoding.UTF8.GetBytes(jsonData);

            bytes = await HandleCompressionAsync(bytes, compressionOption);
            return Result<string>.Success(HandleEncoding(bytes, encodingOption));
        }
        catch (Exception ex)
        {
            return LogAndReturnFailure<string>($"JSON Serialization failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<T>> DeserializeAsync<T>(string data, CompressionOption compressionOption,
        EncodingOption encodingOption)
    {
        if (string.IsNullOrWhiteSpace(data))
            return LogAndReturnFailure<T>("DeserializeJsonAsync: Input string is null or whitespace.");

        try
        {
            var bytes = HandleDecoding(data, encodingOption);
            bytes = await HandleCompressionAsync(bytes, compressionOption, false);

            var jsonData = Encoding.UTF8.GetString(bytes);
            var result = JsonConvert.DeserializeObject<T>(jsonData);
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return LogAndReturnFailure<T>($"JSON Deserialization failed: {ex.Message}");
        }
    }

    /// <summary>
    ///     Handles compression asynchronously based on the specified compression option.
    /// </summary>
    /// <param name="data">The data to compress or decompress.</param>
    /// <param name="option">The compression option.</param>
    /// <param name="compress">Determines whether to compress or decompress the data.</param>
    /// <returns>A task that represents the asynchronous operation, containing the processed data.</returns>
    private async Task<byte[]?> HandleCompressionAsync(byte[]? data, CompressionOption option, bool compress = true)
    {
        return option switch
        {
            CompressionOption.Compressed => compress
                ? await _compressionHelper.CompressAsync(data, CompressionType.Brotli)
                : await _compressionHelper.DecompressAsync(data, CompressionType.Brotli),
            CompressionOption.None => data,
            _ => throw new ArgumentException("Invalid compression option.", nameof(option))
        };
    }

    /// <summary>
    ///     Handles encoding of byte data to a string based on the specified encoding option.
    /// </summary>
    /// <param name="data">The byte array to encode.</param>
    /// <param name="option">The encoding option.</param>
    /// <returns>The encoded string.</returns>
    private string HandleEncoding(byte[]? data, EncodingOption option)
    {
        return option switch
        {
            EncodingOption.Base64 => Convert.ToBase64String(data),
            EncodingOption.Plain => Encoding.UTF8.GetString(data),
            _ => throw new ArgumentException("Invalid encoding option.", nameof(option))
        };
    }

    /// <summary>
    ///     Handles decoding of a string to byte data based on the specified encoding option.
    /// </summary>
    /// <param name="data">The string to decode.</param>
    /// <param name="option">The encoding option.</param>
    /// <returns>The decoded byte array.</returns>
    private byte[]? HandleDecoding(string data, EncodingOption option)
    {
        return option switch
        {
            EncodingOption.Base64 => Convert.FromBase64String(data),
            EncodingOption.Plain => Encoding.UTF8.GetBytes(data),
            _ => throw new ArgumentException("Invalid encoding option.", nameof(option))
        };
    }

    /// <summary>
    ///     Logs an error message and returns a failure result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="message">The error message to log.</param>
    /// <returns>A failure result containing the error message.</returns>
    private Result<T> LogAndReturnFailure<T>(string message)
    {
        _logger.LogError(message);
        return Result<T>.Failure(message);
    }
}
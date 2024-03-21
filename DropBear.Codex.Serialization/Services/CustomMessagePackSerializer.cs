using Cysharp.Text;
using DropBear.Codex.AppLogger.Interfaces;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Provides serialization and deserialization services using MessagePack with optional LZ4 compression.
/// </summary>
public class CustomMessagePackSerializer : IMessagePackSerializer
{
    private readonly IAppLogger<CustomMessagePackSerializer> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomMessagePackSerializer" /> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging errors or information.</param>
    /// <exception cref="ArgumentNullException">Thrown if the logger is null.</exception>
    public CustomMessagePackSerializer(IAppLogger<CustomMessagePackSerializer> logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

    /// <summary>
    ///     Serializes the given data to a byte array using MessagePack with optional LZ4 compression.
    /// </summary>
    /// <typeparam name="T">The type of the data to serialize.</typeparam>
    /// <param name="data">The data to serialize. Must not be null.</param>
    /// <param name="compressionOption">Specifies whether to apply LZ4 compression.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous serialization operation, containing the result as a byte array.</returns>
    public async Task<Result<byte[]>> SerializeAsync<T>(T? data, CompressionOption compressionOption,
        CancellationToken cancellationToken = default) where T : notnull
    {
        if (data is null) return LogAndReturnFailure<byte[]>("SerializeAsync: Input data is null.");

        try
        {
            var options = GetSerializerOptions(compressionOption);
            using var memoryStream = new MemoryStream();
            await MessagePackSerializer.SerializeAsync(memoryStream, data, options, cancellationToken)
                .ConfigureAwait(false);
            return Result<byte[]>.Success(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            return LogAndReturnFailure<byte[]>(ZString.Format("MessagePack Serialization failed: {0}", ex.Message));
        }
    }

    /// <summary>
    ///     Deserializes the given byte array back into an instance of type T using MessagePack, with optional LZ4
    ///     decompression.
    /// </summary>
    /// <typeparam name="T">The type into which to deserialize the data.</typeparam>
    /// <param name="data">The byte array to deserialize. Must not be null or empty.</param>
    /// <param name="compressionOption">Specifies whether the data was compressed with LZ4 and should be decompressed.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous deserialization operation, containing the deserialized data.</returns>
    public async Task<Result<T>> DeserializeAsync<T>(byte[]? data, CompressionOption compressionOption,
        CancellationToken cancellationToken = default) where T : notnull
    {
        if (data is null || data.Length is 0)
            return LogAndReturnFailure<T>("DeserializeAsync: Input data is null or empty.");

        try
        {
            var options = GetSerializerOptions(compressionOption);
            using var memoryStream = new MemoryStream(data);
            var result = await MessagePackSerializer.DeserializeAsync<T>(memoryStream, options, cancellationToken)
                .ConfigureAwait(false);
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return LogAndReturnFailure<T>(ZString.Format("MessagePack Deserialization failed: {0}", ex.Message));
        }
    }

    /// <summary>
    ///     Logs the specified message as an error and returns a failure result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="message">The error message to log.</param>
    /// <returns>A failure result containing the error message.</returns>
    private Result<T> LogAndReturnFailure<T>(string message) where T : notnull
    {
        _logger.LogError(message);
        return Result<T>.Failure(message);
    }

    /// <summary>
    ///     Constructs the serializer options based on the specified compression option.
    /// </summary>
    /// <param name="compressionOption">Specifies the compression option to use.</param>
    /// <returns>The serializer options configured with the appropriate compression settings.</returns>
    private static MessagePackSerializerOptions GetSerializerOptions(CompressionOption compressionOption) =>
        compressionOption switch
        {
            CompressionOption.None => MessagePackSerializerOptions.Standard,
            CompressionOption.Compressed => MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression
                .Lz4BlockArray),
            _ => throw new ArgumentOutOfRangeException(nameof(compressionOption), "Invalid compression option.")
        };
}

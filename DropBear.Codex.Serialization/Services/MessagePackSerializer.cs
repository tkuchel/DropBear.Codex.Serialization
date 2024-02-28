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
public class MessagePackSerializer : IMessagePackSerializer
{
    private readonly ILogger<MessagePackSerializer> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessagePackSerializer" /> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging error or information.</param>
    /// <exception cref="ArgumentNullException">Thrown if the logger is null.</exception>
    public MessagePackSerializer(ILogger<MessagePackSerializer> logger) => _logger =
        logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

    /// <inheritdoc />
    public async Task<Result<byte[]>> SerializeAsync<T>(T data, CompressionOption compressionOption,
        CancellationToken cancellationToken = default)
    {
        if (data == null) return LogAndReturnFailure<byte[]>("SerializeAsync: Input data is null.");

        try
        {
            var options = GetSerializerOptions(compressionOption);

            using var memoryStream = new MemoryStream();
            await MessagePack.MessagePackSerializer.SerializeAsync(memoryStream, data, options, cancellationToken).ConfigureAwait(false);
            return Result<byte[]>.Success(memoryStream.ToArray());
        }
        catch (Exception ex)
        {
            return LogAndReturnFailure<byte[]>($"MessagePack Serialization failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<T>> DeserializeAsync<T>(byte[]? data, CompressionOption compressionOption,
        CancellationToken cancellationToken = default) where T : notnull
    {
        if (data == null || data.Length == 0)
            return LogAndReturnFailure<T>("DeserializeAsync: Input data is null or empty.");

        try
        {
            var options = GetSerializerOptions(compressionOption);

            using var memoryStream = new MemoryStream(data);
            var result =
                await MessagePack.MessagePackSerializer.DeserializeAsync<T>(memoryStream, options, cancellationToken).ConfigureAwait(false);
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return LogAndReturnFailure<T>($"MessagePack Deserialization failed: {ex.Message}");
        }
    }

    /// <summary>
    ///     Logs the specified message as an error and returns a failure result.
    /// </summary>
    private Result<T> LogAndReturnFailure<T>(string? message) where T : notnull
    {
#pragma warning disable CA1848
        if (message is not null)
        {
            _logger.ZLogError($"{message}");
#pragma warning restore CA1848
            return Result<T>.Failure(message);
        } else
        {
            _logger.ZLogError($"An error occurred during serialization or deserialization.");
            return Result<T>.Failure("An error occurred during serialization or deserialization.");
        }
    }

    /// <summary>
    ///     Constructs the serializer options based on the specified compression option.
    /// </summary>
    private static MessagePackSerializerOptions GetSerializerOptions(CompressionOption compressionOption)
    {
        var options = MessagePackSerializerOptions.Standard;
        return compressionOption switch
        {
            CompressionOption.None => options,
            CompressionOption.Compressed => options.WithCompression(MessagePackCompression.Lz4BlockArray),
            _ => throw new ArgumentOutOfRangeException(nameof(compressionOption), "Invalid compression option.")
        };
    }
}

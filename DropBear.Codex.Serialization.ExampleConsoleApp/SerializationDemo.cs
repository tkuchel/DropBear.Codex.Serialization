using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Serialization.ConsoleApp;

public class SerializationDemo(IDataSerializer dataSerializer, ILogger<SerializationDemo> logger)
{
    public async Task ExecuteDemo()
    {
        var testData = new TestData { Id = 1, Name = "Test Data", Created = DateTime.UtcNow };

        var memoryPackData = new MemoryPackTestData { Id = 1, Name = "Test Data", Created = DateTime.UtcNow };

        await SerializeAndDeserialize(testData).ConfigureAwait(false);
        await SerializeAndDeserialize(memoryPackData).ConfigureAwait(false);
    }

    private async Task SerializeAndDeserialize<T>(T data) where T : class
    {
        try
        {
            // Serialize to JSON
            var jsonResult =
                await dataSerializer.SerializeJsonAsync(data, CompressionOption.Compressed, EncodingOption.Base64)
                    .ConfigureAwait(false);
            if (jsonResult is { IsSuccess: true })
            {
                logger.ZLogInformation($"JSON serialization successful for {typeof(T).Name}");
            }
            else
            {
                logger.ZLogError($"JSON serialization failed for {typeof(T).Name}: {jsonResult?.ErrorMessage}");
                return;
            }

            // Deserialize from JSON
            var deserializedFromJson = await dataSerializer.DeserializeJsonAsync<T>(jsonResult.Value,
                CompressionOption.Compressed, EncodingOption.Base64).ConfigureAwait(false);
            if (deserializedFromJson is { IsSuccess: true })
            {
                logger.ZLogInformation($"JSON deserialization successful for {typeof(T).Name}");
            }
            else
            {
                logger.ZLogError(
                    $"JSON deserialization failed for {typeof(T).Name}: {deserializedFromJson?.ErrorMessage}");
                return;
            }

            // MessagePack serialization
            var messagePackSerializable = await dataSerializer.IsMessagePackSerializable<T>().ConfigureAwait(false);
            if (!messagePackSerializable.IsSuccess) return;
            if (messagePackSerializable is { IsSuccess: true, Value: true })
            {
                var messagePackResult =
                    await dataSerializer.SerializeMessagePackAsync(data, CompressionOption.Compressed)
                        .ConfigureAwait(false);
                if (messagePackResult.IsSuccess)
                {
                    logger.ZLogInformation($"MessagePack serialization successful for {typeof(T).Name}");
                    var deserializedMessagePack =
                        await dataSerializer.DeserializeMessagePackAsync<T>(messagePackResult.Value,
                            CompressionOption.Compressed).ConfigureAwait(false);
                    if (deserializedMessagePack.IsSuccess)
                        logger.ZLogInformation($"MessagePack deserialization successful for {typeof(T).Name}");
                    else
                        logger.ZLogError(
                            $"MessagePack deserialization failed for {typeof(T).Name}: {deserializedMessagePack.ErrorMessage}");
                }
                else
                {
                    logger.ZLogError(
                        $"MessagePack serialization failed for {typeof(T).Name}: {messagePackResult.ErrorMessage}");
                }
            }
            else
            {
                logger.ZLogWarning($"Type {typeof(T).Name} is not serializable with MessagePack.");
            }

            // MemoryPack serialization
            var memoryPackSerializable = await dataSerializer.IsMemoryPackSerializable<T>().ConfigureAwait(false);
            if (!memoryPackSerializable.IsSuccess) return;
            var memoryPackResult = await dataSerializer.SerializeMemoryPackAsync(data, CompressionOption.Compressed)
                .ConfigureAwait(false);
            if (memoryPackResult.IsSuccess)
            {
                logger.ZLogInformation($"MemoryPack serialization successful for {typeof(T).Name}");
                var deserializedMemoryPack =
                    await dataSerializer.DeserializeMemoryPackAsync<T>(memoryPackResult.Value,
                        CompressionOption.Compressed).ConfigureAwait(false);
                if (deserializedMemoryPack.IsSuccess)
                    logger.ZLogInformation($"MemoryPack deserialization successful for {typeof(T).Name}");
                else
                    logger.ZLogError(
                        $"MemoryPack deserialization failed for {typeof(T).Name}: {deserializedMemoryPack.ErrorMessage}");
            }
            else
            {
                logger.ZLogError(
                    $"MemoryPack serialization failed for {typeof(T).Name}: {memoryPackResult.ErrorMessage}");
            }

            logger.ZLogInformation($"Serialization and deserialization demo completed for {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            logger.ZLogError($"An error occurred during serialization/deserialization: {ex.Message}");
        }
    }
}

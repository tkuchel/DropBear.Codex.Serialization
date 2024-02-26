using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using MemoryPack;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

namespace DropBear.Codex.Serialization.ConsoleApp;

internal class Program
{
    private static async Task Main()
    {
        var serviceProvider = ConfigureServices();
        await RunSerializationDemo(serviceProvider);
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddDataSerializationServices();
        services.AddTransient<SerializationDemo>();
        ConfigureZLogger(services);
        return services.BuildServiceProvider();
    }

    private static async Task RunSerializationDemo(IServiceProvider serviceProvider)
    {
        var serializerDemo = serviceProvider.GetRequiredService<SerializationDemo>();
        await serializerDemo.ExecuteDemo();
    }

    /// <summary>
    ///     Configures ZLogger logging services.
    /// </summary>
    /// <param name="services">The IServiceCollection to add logging services to.</param>
    private static void ConfigureZLogger(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders()
                .SetMinimumLevel(LogLevel.Debug)
                .AddZLoggerConsole(options =>
                {
                    options.UseJsonFormatter(formatter =>
                    {
                        formatter.IncludeProperties = IncludeProperties.Timestamp | IncludeProperties.LogLevel |
                                                      IncludeProperties.FilePath | IncludeProperties.CategoryName |
                                                      IncludeProperties.Message;
                    });
                })
                .AddZLoggerRollingFile(options =>
                {
                    options.FilePathSelector = (timestamp, sequenceNumber) =>
                        $"logs/{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:000}.log";
                    options.RollingInterval = RollingInterval.Day;
                    options.RollingSizeKB = 1024; // 1MB
                });
        });
    }
}

public class SerializationDemo
{
    private readonly IDataSerializer _dataSerializer;
    private readonly ILogger<SerializationDemo> _logger;

    public SerializationDemo(IDataSerializer dataSerializer, ILogger<SerializationDemo> logger)
    {
        _dataSerializer = dataSerializer;
        _logger = logger;
    }

    public async Task ExecuteDemo()
    {
        var testData = new TestData
        {
            Id = 1,
            Name = "Test Data",
            Created = DateTime.UtcNow
        };

        var memoryPackData = new MemoryPackTestData
        {
            Id = 1,
            Name = "Test Data",
            Created = DateTime.UtcNow
        };

        await SerializeAndDeserialize(testData);
        await SerializeAndDeserialize(memoryPackData);
    }

    public async Task SerializeAndDeserialize<T>(T data) where T : class
    {
        try
        {
            // Serialize to JSON
            var jsonResult =
                await _dataSerializer.SerializeJsonAsync(data, CompressionOption.Compressed, EncodingOption.Base64);
            if (jsonResult.IsSuccess)
            {
                _logger.ZLogInformation($"JSON serialization successful for {typeof(T).Name}");
            }
            else
            {
                _logger.ZLogError($"JSON serialization failed for {typeof(T).Name}: {jsonResult.ErrorMessage}");
                return;
            }

            // Deserialize from JSON
            var deserializedFromJson = await _dataSerializer.DeserializeJsonAsync<T>(jsonResult.Value,
                CompressionOption.Compressed, EncodingOption.Base64);
            if (deserializedFromJson.IsSuccess)
            {
                _logger.ZLogInformation($"JSON deserialization successful for {typeof(T).Name}");
            }
            else
            {
                _logger.ZLogError(
                    $"JSON deserialization failed for {typeof(T).Name}: {deserializedFromJson.ErrorMessage}");
                return;
            }

            // MessagePack serialization
            var messagePackSerializable = await _dataSerializer.IsMessagePackSerializable<T>();
            if (!messagePackSerializable.IsSuccess) return;
            if (messagePackSerializable is { IsSuccess: true, Value: true })
            {
                var messagePackResult =
                    await _dataSerializer.SerializeMessagePackAsync(data, CompressionOption.Compressed);
                if (messagePackResult.IsSuccess)
                {
                    _logger.ZLogInformation($"MessagePack serialization successful for {typeof(T).Name}");
                    var deserializedMessagePack =
                        await _dataSerializer.DeserializeMessagePackAsync<T>(messagePackResult.Value,
                            CompressionOption.Compressed);
                    if (deserializedMessagePack.IsSuccess)
                        _logger.ZLogInformation($"MessagePack deserialization successful for {typeof(T).Name}");
                    else
                        _logger.ZLogError(
                            $"MessagePack deserialization failed for {typeof(T).Name}: {deserializedMessagePack.ErrorMessage}");
                }
                else
                {
                    _logger.ZLogError(
                        $"MessagePack serialization failed for {typeof(T).Name}: {messagePackResult.ErrorMessage}");
                }
            }
            else
            {
                _logger.LogWarning($"Type {typeof(T).Name} is not serializable with MessagePack.");
            }

            // MemoryPack serialization
            var memoryPackSerializable = await _dataSerializer.IsMemoryPackSerializable<T>();
            if (!memoryPackSerializable.IsSuccess) return;
            var memoryPackResult = await _dataSerializer.SerializeMemoryPackAsync(data, CompressionOption.Compressed);
            if (memoryPackResult.IsSuccess)
            {
                _logger.ZLogInformation($"MemoryPack serialization successful for {typeof(T).Name}");
                var deserializedMemoryPack =
                    await _dataSerializer.DeserializeMemoryPackAsync<T>(memoryPackResult.Value,
                        CompressionOption.Compressed);
                if (deserializedMemoryPack.IsSuccess)
                    _logger.ZLogInformation($"MemoryPack deserialization successful for {typeof(T).Name}");
                else
                    _logger.ZLogError(
                        $"MemoryPack deserialization failed for {typeof(T).Name}: {deserializedMemoryPack.ErrorMessage}");
            }
            else
            {
                _logger.ZLogError(
                    $"MemoryPack serialization failed for {typeof(T).Name}: {memoryPackResult.ErrorMessage}");
            }

            _logger.ZLogInformation($"Serialization and deserialization demo completed for {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            _logger.ZLogError($"An error occurred during serialization/deserialization: {ex.Message}");
        }
    }
}

[MemoryPackable]
public partial class MemoryPackTestData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
}

[MessagePackObject]
public class TestData
{
    [Key(0)] public int Id { get; set; }

    [Key(1)] public string Name { get; set; }

    [Key(2)] public DateTime Created { get; set; }
}
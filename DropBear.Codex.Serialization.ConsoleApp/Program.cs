using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.Serialization.ConsoleApp;

internal class Program
{
    private static async Task Main()
    {
        // Setup DI
        var services = new ServiceCollection();

        // Configure Services
        ConfigureServices(services);
        
        // Add Logging
        services.AddLogging(builder => builder.AddConsole());

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Use the data serialization service to serialize and deserialize data
        var dataSerializer = serviceProvider.GetRequiredService<IDataSerializer>();

        // Serialize and deserialize some data
        var data = new TestData
        {
            Id = 1,
            Name = "Test Data",
            Created = DateTime.UtcNow
        };

        var json = await dataSerializer.SerializeJsonAsync(data, CompressionOption.Compressed, EncodingOption.Base64);
        
        var messagePack = await dataSerializer.SerializeMessagePackAsync(data, CompressionOption.Compressed);
        
        var memoryPack = await dataSerializer.SerializeMemoryPackAsync(data, CompressionOption.Compressed);

        if (json.IsFailure)
        {
            Console.WriteLine($"Failed to serialize data: {json.ErrorMessage}");
            return;
        }

        if (messagePack.IsFailure)
        {
            Console.WriteLine($"Failed to serialize data: {messagePack.ErrorMessage}");
            return;
        }
        
        if (memoryPack.IsFailure)
        {
            Console.WriteLine($"Failed to serialize data: {memoryPack.ErrorMessage}");
            return;
        }
        
        var deserializedData = await dataSerializer.DeserializeJsonAsync<TestData>(json.Value, CompressionOption.Compressed, EncodingOption.Base64);
        
        var deserializedMessagePackData = await dataSerializer.DeserializeMessagePackAsync<TestData>(messagePack.Value, CompressionOption.Compressed);
        
        var deserializedMemoryPackData = await dataSerializer.DeserializeMemoryPackAsync<TestData>(memoryPack.Value, CompressionOption.Compressed);
        
        if (deserializedData.IsFailure)
        {
            Console.WriteLine($"Failed to deserialize data: {deserializedData.ErrorMessage}");
            return;
        }
        
        if (deserializedMessagePackData.IsFailure)
        {
            Console.WriteLine($"Failed to deserialize data: {deserializedMessagePackData.ErrorMessage}");
            return;
        }
        
        if (deserializedMemoryPackData.IsFailure)
        {
            Console.WriteLine($"Failed to deserialize data: {deserializedMemoryPackData.ErrorMessage}");
            return;
        }
        
        Console.WriteLine($"Data: {deserializedData.Value.Id}, {deserializedData.Value.Name}, {deserializedData.Value.Created}");
        
        Console.WriteLine($"Data: {deserializedMessagePackData.Value.Id}, {deserializedMessagePackData.Value.Name}, {deserializedMessagePackData.Value.Created}");
        
        Console.WriteLine($"Data: {deserializedMemoryPackData.Value.Id}, {deserializedMemoryPackData.Value.Name}, {deserializedMemoryPackData.Value.Created}");
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSerializationServices();
    }

    [MessagePackObject]
    private class TestData
    {
        [Key(0)]
        public int Id { get; set; }
       
        [Key(1)]
        public string Name { get; set; }
        
        [Key(2)]
        public DateTime Created { get; set; }
    }
}
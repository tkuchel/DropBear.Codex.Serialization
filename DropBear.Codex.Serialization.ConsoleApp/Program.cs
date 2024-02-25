using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Enums;
using DropBear.Codex.Serialization.Interfaces;
using MemoryPack;
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
        
        // Serialize and deserialize some data MemoryPack
        var memoryPackData = new MemoryPackTestData
        {
            Id = 1,
            Name = "Test Data",
            Created = DateTime.UtcNow
        };

        var json = await dataSerializer.SerializeJsonAsync(data, CompressionOption.Compressed, EncodingOption.Base64);

        var messagePackCheck = await dataSerializer.IsMessagePackSerializable<TestData>();
        var messagePack = default(Result<byte[]>);
        if (messagePackCheck.Value)
        {
            messagePack = await dataSerializer.SerializeMessagePackAsync(data, CompressionOption.Compressed);
        }else
        {
            Console.WriteLine("Type is not serializable with MessagePack");
        }

        
        var memoryPack = await dataSerializer.SerializeMemoryPackAsync(memoryPackData, CompressionOption.Compressed);

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
        
        var deserializedMemoryPackData = await dataSerializer.DeserializeMemoryPackAsync<MemoryPackTestData>(memoryPack.Value, CompressionOption.Compressed);
        
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

    }

[MemoryPackable]
public partial class MemoryPackTestData // TODO: we are going to need a similar checker to the messagepack one to make sure this attribute is added.
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
}
    
[MessagePackObject]
public class TestData //TODO: This has to be public for messagepack to work, need to add a check for this as part of the messagepackchecker method also this cannot be a class within a class.
{
    [Key(0)]
    public int Id { get; set; }
       
    [Key(1)]
    public string Name { get; set; }
        
    [Key(2)]
    public DateTime Created { get; set; }
}

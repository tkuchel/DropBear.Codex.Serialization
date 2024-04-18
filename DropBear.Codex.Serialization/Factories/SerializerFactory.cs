using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Interfaces;
using DropBear.Codex.Serialization.Serializers;

namespace DropBear.Codex.Serialization.Factories;

public abstract class SerializerFactory
{
    internal static readonly Dictionary<Type, Type> RegisteredSerializers = new();

    public static ISerializer CreateSerializer(SerializationConfig config)
    {
        if (RegisteredSerializers.TryGetValue(config.SerializerType, out var serializerType))
        {
            if (serializerType is null)
                throw new ArgumentException($"Serializer type '{config.SerializerType.FullName}' is not registered.",
                    nameof(config));

            return (ISerializer)Activator.CreateInstance(serializerType, config.RecyclableMemoryStreamManager) ??
                   throw new InvalidOperationException("Failed to create serializer instance.");
        }

        // Handle built-in serializers
        if (config.SerializerType == typeof(JsonSerializer))
            return new JsonSerializer(config.JsonSerializerOptions, config.RecyclableMemoryStreamManager);
        else if (config.SerializerType == typeof(MessagePackSerializer))
            return new MessagePackSerializer(config.MessagePackSerializerOptions, config.RecyclableMemoryStreamManager);
        else
            throw new ArgumentException("Invalid serializer type specified.", nameof(config));
    }
}

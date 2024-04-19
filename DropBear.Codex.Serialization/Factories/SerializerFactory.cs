using System.Collections.Concurrent;
using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Interfaces;
using DropBear.Codex.Serialization.Serializers;

namespace DropBear.Codex.Serialization.Factories;

/// <summary>
///     Factory class for creating serializers based on provided configuration.
/// </summary>
public abstract class SerializerFactory
{
    internal static readonly ConcurrentDictionary<Type, Type> RegisteredSerializers = new();

    /// <summary>
    ///     Creates a serializer based on the provided configuration.
    /// </summary>
    /// <param name="config">The serialization configuration.</param>
    /// <returns>An instance of <see cref="ISerializer" />.</returns>
    public static ISerializer CreateSerializer(SerializationConfig config)
    {
        // Validate configuration before processing
        ValidateConfiguration(config);

        // First, create the base serializer according to the specified type
        var serializer = CreateBaseSerializer(config);

        // Apply transformations in the correct order
        serializer = ApplyCompression(serializer, config);
        serializer = ApplyEncryption(serializer, config);
        serializer = ApplyEncoding(serializer, config);

        return serializer;
    }

    private static void ValidateConfiguration(SerializationConfig config)
    {
        if (config.SerializerType is null)
            throw new ArgumentException("Serializer type must be specified.", nameof(config));
        if (config.RecyclableMemoryStreamManager is null)
            throw new ArgumentException("RecyclableMemoryStreamManager must be specified.", nameof(config));
    }

    private static ISerializer CreateBaseSerializer(SerializationConfig config)
    {
        if(config.SerializerType is null)
            throw new ArgumentException("Serializer type must be specified.", nameof(config));
        
        if(config.RecyclableMemoryStreamManager is null)
            throw new ArgumentException("RecyclableMemoryStreamManager must be specified.", nameof(config));
        
        if (!RegisteredSerializers.TryGetValue(config.SerializerType, out var serializerType))
            return CreateBuiltInSerializer(config);

        var serializer = (ISerializer)Activator.CreateInstance(serializerType, config.RecyclableMemoryStreamManager)!;
        if (serializer is null)
            throw new InvalidOperationException("Failed to create serializer instance.");

        return serializer;
    }

    private static ISerializer ApplyCompression(ISerializer serializer, SerializationConfig config)
    {
        if (config.CompressionProvider is null) return serializer;
        var compressor = config.CompressionProvider.GetCompressor();
        return new CompressedSerializer(serializer, compressor);
    }

    private static ISerializer ApplyEncryption(ISerializer serializer, SerializationConfig config)
    {
        if (config.EncryptionProvider is null) return serializer;
        var encryptor = config.EncryptionProvider.GetEncryptor();
        return new EncryptedSerializer(serializer, encryptor);
    }

    private static ISerializer ApplyEncoding(ISerializer serializer, SerializationConfig config)
    {
        if (config.EncodingProvider is null) return serializer;
        var encodingProvider = config.EncodingProvider;
        return new EncodedSerializer(serializer, encodingProvider);
    }

    private static ISerializer CreateBuiltInSerializer(SerializationConfig config)
    {
        if(config.RecyclableMemoryStreamManager is null)
            throw new ArgumentException("RecyclableMemoryStreamManager must be specified.", nameof(config));
        
        switch (config.SerializerType)
        {
            case { } t when t == typeof(JsonSerializer):
                if (config.JsonSerializerOptions is null)
                    throw new ArgumentException("JsonSerializerOptions must be specified for JsonSerializer.",
                        nameof(config));
                return new JsonSerializer(config.JsonSerializerOptions, config.RecyclableMemoryStreamManager);

            case { } t when t == typeof(MessagePackSerializer):
                if (config.MessagePackSerializerOptions is null)
                    throw new ArgumentException(
                        "MessagePackSerializerOptions must be specified for MessagePackSerializer.", nameof(config));
                return new MessagePackSerializer(config.MessagePackSerializerOptions,
                    config.RecyclableMemoryStreamManager);

            default:
                throw new ArgumentException("Invalid serializer type specified.", nameof(config));
        }
    }
}

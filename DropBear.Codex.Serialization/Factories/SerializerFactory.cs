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
        // First, check if a StreamSerializer is specified in the config
        if (config.StreamSerializer is not null)
        {
            // Assume StreamSerializer is correctly set to an instance of IStreamSerializer
            return new StreamSerializerAdapter(config.StreamSerializer);
        }

        // Proceed with creating a serializer based on the SerializerType
        switch (config.SerializerType)
        {
            case { } t when t == typeof(JsonSerializer):
                if (config.JsonSerializerOptions is not null)
                {
                    var jsonStreamSerializer = new JsonStreamSerializer(config.JsonSerializerOptions);
                    return new StreamSerializerAdapter(jsonStreamSerializer);
                }

                break;

            case { } t when t == typeof(MessagePackSerializer):
                if (config.MessagePackSerializerOptions is not null)
                    return new MessagePackSerializer(config.MessagePackSerializerOptions,
                        config.RecyclableMemoryStreamManager);
                break;

            case { } t when t == typeof(CombinedSerializer):
                if (config.JsonSerializerOptions is not null)
                {
                    var defaultSerializer = new Serializers.JsonSerializer(config.JsonSerializerOptions,
                        config.RecyclableMemoryStreamManager);
                    var streamSerializer = new JsonStreamSerializer(config.JsonSerializerOptions);
                    return new CombinedSerializer(defaultSerializer, streamSerializer);
                }

                break;

            default:
                throw new ArgumentException("Invalid serializer type specified.", nameof(config));
        }

        throw new ArgumentException("Serializer options must be specified for the selected serializer type.",
            nameof(config));
    }
}

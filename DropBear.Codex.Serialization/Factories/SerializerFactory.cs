using System.Collections.Concurrent;
using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Interfaces;
using DropBear.Codex.Serialization.Serializers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DropBear.Codex.Serialization.Factories;

public abstract class SerializerFactory
{
    internal static readonly ConcurrentDictionary<Type, Type> RegisteredSerializers = new();

    public static ISerializer CreateSerializer(SerializationConfig config)
    {
        ValidateConfiguration(config);
        var serializer = CreateBaseSerializer(config);

        serializer = ApplyCompression(serializer, config);
        serializer = ApplyEncryption(serializer, config);
        serializer = ApplyEncoding(serializer, config);

        return serializer;
    }

    private static void ValidateConfiguration(SerializationConfig config)
    {
        if (config.SerializerType is null)
            throw new ArgumentException("Serializer type must be specified.", nameof(config));
    }

    private static ISerializer CreateBaseSerializer(SerializationConfig config)
    {
        if (config.SerializerType is null)
            throw new ArgumentException("Serializer type must be specified.", nameof(config));

        if (!RegisteredSerializers.TryGetValue(config.SerializerType, out var serializerType))
            return CreateBuiltInSerializer(config);

        return (ISerializer)Activator.CreateInstance(serializerType, config.RecyclableMemoryStreamManager)!;
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


    private static ISerializer ApplyCompression(ISerializer serializer, SerializationConfig config) =>
        config.CompressionProvider is not null
            ? new CompressedSerializer(serializer, config.CompressionProvider.GetCompressor())
            : serializer;

    private static ISerializer ApplyEncryption(ISerializer serializer, SerializationConfig config) =>
        config.EncryptionProvider is not null
            ? new EncryptedSerializer(serializer, config.EncryptionProvider.GetEncryptor())
            : serializer;

    private static ISerializer ApplyEncoding(ISerializer serializer, SerializationConfig config) =>
        config.EncodingProvider is not null ? new EncodedSerializer(serializer, config.EncodingProvider) : serializer;
}

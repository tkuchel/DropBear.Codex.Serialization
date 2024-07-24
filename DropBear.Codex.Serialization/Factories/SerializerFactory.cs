#region

using System.Runtime.Versioning;
using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Interfaces;
using DropBear.Codex.Serialization.Serializers;

#endregion

namespace DropBear.Codex.Serialization.Factories;

[SupportedOSPlatform("windows")]
public abstract class SerializerFactory
{
#pragma warning disable MA0015
    private static SerializationConfig? s_config;

    public static ISerializer CreateSerializer(SerializationConfig? config)
    {
        s_config = config ?? throw new ArgumentNullException(nameof(config), "Configuration must be provided.");
        ValidateConfiguration();

        var serializer = CreateBaseSerializer();
        serializer = ApplyCompression(serializer);
        serializer = ApplyEncryption(serializer);
        return ApplyEncoding(serializer);
    }

    private static void ValidateConfiguration()
    {
        if (s_config?.SerializerType == null)
        {
            throw new ArgumentException("Serializer type must be specified.", nameof(s_config.SerializerType));
        }

        if (s_config.RecyclableMemoryStreamManager is null)
        {
            throw new ArgumentException("RecyclableMemoryStreamManager must be specified.",
                nameof(s_config.RecyclableMemoryStreamManager));
        }
    }

    private static ISerializer CreateBaseSerializer()
    {
        var serializerType =
            s_config?.SerializerType ?? throw new InvalidOperationException("Serializer type not set.");

        return InstantiateSerializer(serializerType);
    }

    private static ISerializer InstantiateSerializer(Type serializerType)
    {
        var constructor = serializerType.GetConstructor(new[] { typeof(SerializationConfig) })
                          ?? throw new InvalidOperationException(
                              $"No suitable constructor found for {serializerType.FullName}.");
        if (s_config is not null)
        {
            return (ISerializer)constructor.Invoke(new object[] { s_config });
        }

        throw new InvalidOperationException("Configuration must be provided.");
    }

    private static ISerializer ApplyCompression(ISerializer serializer)
    {
        if (s_config?.CompressionProviderType == null)
        {
            return serializer;
        }

        var compressor = (ICompressionProvider)CreateProvider(s_config.CompressionProviderType);
        return new CompressedSerializer(serializer, compressor);
    }

    private static ISerializer ApplyEncryption(ISerializer serializer)
    {
        if (s_config?.EncryptionProviderType == null)
        {
            return serializer;
        }

        var encryptor = (IEncryptionProvider)CreateProvider(s_config.EncryptionProviderType);
        return new EncryptedSerializer(serializer, encryptor);
    }

    private static ISerializer ApplyEncoding(ISerializer serializer)
    {
        if (s_config?.EncodingProviderType == null)
        {
            return serializer;
        }

        var encoder = (IEncodingProvider)CreateProvider(s_config.EncodingProviderType);
        return new EncodedSerializer(serializer, encoder);
    }

    private static object CreateProvider(Type providerType)
    {
        var constructor = providerType.GetConstructor(new[] { typeof(SerializationConfig) })
                          ?? throw new InvalidOperationException(
                              $"No suitable constructor found for {providerType.FullName}.");
        if (s_config is not null)
        {
            return constructor.Invoke(new object[] { s_config });
        }

        throw new InvalidOperationException("Configuration must be provided.");
    }
#pragma warning restore MA0015
}

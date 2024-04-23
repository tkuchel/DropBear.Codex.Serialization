using System.Runtime.Versioning;
using System.Text.Json;
using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Factories;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;
using MessagePack.Resolvers;

namespace DropBear.Codex.Serialization.Extensions;

[SupportedOSPlatform("windows")]
public static class SerializerFactoryExtensions
{
    public static SerializationBuilder WithDefaultJsonOptions(this SerializationBuilder builder,
        bool writeIndented = false)
    {
        var options = new JsonSerializerOptions { WriteIndented = writeIndented };
        return builder.WithJsonSerializerOptions(options);
    }

    public static SerializationBuilder WithDefaultMessagePackOptions(this SerializationBuilder builder,
        bool resolverEnabled = true)
    {
        var options = MessagePackSerializerOptions.Standard;
        if (resolverEnabled) options = options.WithResolver(StandardResolver.Instance);
        return builder.WithMessagePackSerializerOptions(options);
    }

    public static SerializationBuilder WithAdaptiveCompression(this SerializationBuilder builder,
        Func<Type> providerTypeSelector)
    {
        var providerType = providerTypeSelector();
        if (!typeof(ICompressionProvider).IsAssignableFrom(providerType))
            throw new ArgumentException("Selected type does not implement ICompressionProvider",
                nameof(providerTypeSelector));

        return builder.WithCompression(providerType);
    }

    public static SerializationBuilder WithAdaptiveEncryption(this SerializationBuilder builder,
        Func<Type> providerTypeSelector)
    {
        var providerType = providerTypeSelector();
        if (!typeof(IEncryptionProvider).IsAssignableFrom(providerType))
            throw new ArgumentException("Selected type does not implement IEncryptionProvider",
                nameof(providerTypeSelector));

        return builder.WithEncryption(providerType);
    }

    public static bool ValidateConfiguration(this SerializationConfig config) =>
        config.SerializerType is not null && config.EncodingProviderType is not null &&
        (config.CompressionProviderType is not null || config.EncryptionProviderType is not null);
}

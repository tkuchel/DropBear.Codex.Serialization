using System.Diagnostics;
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
#pragma warning disable IDE0060 // Remove unused parameter
    public static void RegisterSerializer<T>(this SerializerFactory factory) where T : ISerializer =>
        SerializerFactory.RegisteredSerializers.TryAdd(typeof(T), typeof(T));

    public static void RegisterSerializer<TSerializer, TImplementation>()
        where TSerializer : ISerializer
        where TImplementation : class, TSerializer =>
        SerializerFactory.RegisteredSerializers.TryAdd(typeof(TSerializer), typeof(TImplementation));

    public static void UnregisterSerializer<T>(this SerializerFactory factory) where T : ISerializer =>
        SerializerFactory.RegisteredSerializers.TryRemove(typeof(T), out _);


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

    public static void DebugRegisteredSerializers(this SerializerFactory factory)
    {
        foreach (var entry in SerializerFactory.RegisteredSerializers)
            Debug.WriteLine($"Registered: {entry.Key.Name} as {entry.Value.Name}");
    }

    public static SerializationBuilder WithAdaptiveCompression(this SerializationBuilder builder,
        Func<Type> providerTypeSelector)
    {
        var providerType = providerTypeSelector();
        if (!typeof(ICompressionProvider).IsAssignableFrom(providerType))
            throw new ArgumentException("Selected type does not implement ICompressionProvider",
                nameof(providerTypeSelector));

        if (Activator.CreateInstance(providerType) is not ICompressionProvider providerInstance)
            throw new InvalidOperationException("Failed to create an instance of the compression provider.");

        return builder.WithCompression(providerInstance);
    }


    public static SerializationBuilder WithAdaptiveEncryption(this SerializationBuilder builder,
        Func<Type> providerTypeSelector)
    {
        var providerType = providerTypeSelector();
        if (!typeof(IEncryptionProvider).IsAssignableFrom(providerType))
            throw new ArgumentException("Selected type does not implement IEncryptionProvider",
                nameof(providerTypeSelector));

        IEncryptionProvider? providerInstance;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            providerInstance = (IEncryptionProvider)Activator.CreateInstance(providerType);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create an instance of the encryption provider.", ex);
        }

        if (providerInstance is not null) return builder.WithEncryption(providerInstance);
        throw new InvalidOperationException("Failed to create an instance of the encryption provider.");
    }


    public static bool ValidateConfiguration(this SerializationConfig config) =>
        // Implement validation logic, e.g., check all required settings are not null.
        config.SerializerType is not null && config.EncodingProviderType is not null &&
        (config.CompressionProviderType is not null || config.EncryptionProviderType is not null);

#pragma warning restore IDE0060 // Remove unused parameter
}

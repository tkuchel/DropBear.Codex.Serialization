using System.Runtime.Versioning;
using System.Text.Json;
using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Encryption;
using DropBear.Codex.Serialization.Interfaces;
using DropBear.Codex.Serialization.Providers;
using MessagePack;

namespace DropBear.Codex.Serialization.Factories;

/// <summary>
///     Builder class for configuring serialization settings.
/// </summary>
public class SerializationBuilder
{
    private readonly SerializationConfig _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SerializationBuilder" /> class.
    /// </summary>
    public SerializationBuilder() => _config = new SerializationConfig();

    /// <summary>
    ///     Specifies the serializer type to use.
    /// </summary>
    /// <typeparam name="T">The type of serializer.</typeparam>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithSerializer<T>() where T : ISerializer
    {
        _config.SerializerType = typeof(T);
        return this;
    }
    
    /// <summary>
    ///     Specifies the serialization provider to use.
    /// </summary>
    /// <param name="provider">The serialization provider instance.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithSerializer(ISerializer provider)
    {
        _config.SerializerType = provider.GetType();
        return this;
    }
  

    /// <summary>
    ///     Specifies the stream serializer instance to use.
    /// </summary>
    /// <typeparam name="T">The type of stream serializer.</typeparam>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithStreamSerializer<T>() where T : IStreamSerializer, new()
    {
        _config.StreamSerializer = new T();
        return this;
    }

    /// <summary>
    ///     Specifies the compression provider to use.
    /// </summary>
    /// <typeparam name="T">The type of compression provider.</typeparam>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithCompression<T>() where T : ICompressionProvider, new()
    {
        _config.CompressionProvider = new T();
        return this;
    }

    /// <summary>
    ///     Specifies the compression provider to use.
    /// </summary>
    /// <param name="provider">The compression provider instance.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithCompression(ICompressionProvider provider)
    {
        _config.CompressionProvider = provider;
        return this;
    }

    /// <summary>
    ///     Specifies the encoding provider to use.
    /// </summary>
    /// <typeparam name="T">The type of encoding provider.</typeparam>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithEncoding<T>() where T : IEncodingProvider, new()
    {
        _config.EncodingProvider = new T();
        return this;
    }
    
    /// <summary>
    ///     Specifies the encoding provider to use.
    /// </summary>
    /// <param name="provider">The type of encoding provider.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithEncoding(IEncodingProvider provider)
    {
        _config.EncodingProvider = provider;
        return this;
    }

    /// <summary>
    ///     Specifies the encryption provider to use.
    /// </summary>
    /// <param name="publicKeyPath">The path to the public key file.</param>
    /// <param name="privateKeyPath">The path to the private key file.</param>
    /// <returns>The serialization builder instance.</returns>
    [SupportedOSPlatform("windows")]
    public SerializationBuilder WithAesgcmEncryption(string publicKeyPath, string privateKeyPath)
    {
        _config.EncryptionProvider = new AESGCMEncryptionProvider(publicKeyPath, privateKeyPath);
        return this;
    }
    
    /// <summary>
    ///     Specifies the encryption provider to use.
    /// </summary>
    /// <param name="publicKeyPath">The path to the public key file.</param>
    /// <param name="privateKeyPath">The path to the private key file.</param>
    /// <returns>The serialization builder instance.</returns>
    [SupportedOSPlatform("windows")]
    public SerializationBuilder WithAescngEncryption(string publicKeyPath, string privateKeyPath)
    {
        _config.EncryptionProvider = new AESCNGEncryptionProvider(_config.RecyclableMemoryStreamManager,publicKeyPath, privateKeyPath);
        return this;
    }

    /// <summary>
    ///     Specifies the encryption provider to use.
    /// </summary>
    /// <param name="encryptionProvider">The encryption provider instance.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithEncryption(IEncryptionProvider encryptionProvider)
    {
        _config.EncryptionProvider = encryptionProvider;
        return this;
    }

    /// <summary>
    ///     Specifies the JSON serializer options to use.
    /// </summary>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithJsonSerializerOptions(JsonSerializerOptions options)
    {
        _config.SerializerType ??= typeof(DropBear.Codex.Serialization.Serializers.JsonSerializer);
        _config.JsonSerializerOptions = options;
        return this;
    }

    /// <summary>
    ///     Specifies the MessagePack serializer options to use.
    /// </summary>
    /// <param name="options">The MessagePack serializer options.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithMessagePackSerializerOptions(MessagePackSerializerOptions options)
    {
        _config.MessagePackSerializerOptions = options;
        return this;
    }

    /// <summary>
    ///     Sets up a typical configuration that might be a good starting point.
    /// </summary>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithDefaultConfiguration() =>
        WithJsonSerializerOptions(new JsonSerializerOptions { WriteIndented = true })
            .WithCompression<GZipCompressionProvider>();

    /// <summary>
    ///     Builds the serializer based on the configured settings.
    /// </summary>
    /// <returns>The configured serializer instance.</returns>
    public ISerializer Build()
    {
        if (_config.SerializerType == null)
            throw new InvalidOperationException(
                "No serializer type specified. Please specify the serializer type before building.");

        if (_config.SerializerType == typeof(JsonSerializer) && _config.JsonSerializerOptions is null)
            throw new InvalidOperationException("JsonSerializerOptions must be specified for JsonSerializer.");

        return SerializerFactory.CreateSerializer(_config);
    }
}

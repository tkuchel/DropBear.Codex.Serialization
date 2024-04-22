using System.Runtime.Versioning;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Interfaces;
using DropBear.Codex.Serialization.Providers;
using DropBear.Codex.Serialization.Serializers;
using MessagePack;
using MessagePack.ImmutableCollection;
using MessagePack.Resolvers;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MessagePackSerializer = MessagePack.MessagePackSerializer;

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
        _config.EncryptionProvider =
            new AESCNGEncryptionProvider(_config.RecyclableMemoryStreamManager, publicKeyPath, privateKeyPath);
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
        _config.SerializerType ??= typeof(Serializers.JsonSerializer);
        _config.JsonSerializerOptions = options;
        return this;
    }
    
    /// <summary>
    ///     Specifies the JSON serializer options to use.
    /// </summary>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithDefaultJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true, // Indent the output for better readability
            IncludeFields = true, // Include public fields in serialization
            PropertyNameCaseInsensitive = true, // Ignore case when matching property names
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Ignore null values when serializing
            NumberHandling = JsonNumberHandling.AllowReadingFromString, // Allow reading numbers from strings
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Use a more relaxed JSON encoder
            ReferenceHandler = ReferenceHandler.Preserve, // Preserve reference relationships
            MaxDepth = 64, // Set a maximum depth to avoid StackOverflowExceptions
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode, // Handle unknown types as JsonNode
            Converters = {
                new JsonStringEnumConverter(), // Use a converter for enums
                // Add more custom converters here if needed
            }
        };
        _config.SerializerType ??= typeof(Serializers.JsonSerializer);
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
    ///     Specifies default MessagePack serializer options to use.
    /// </summary>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithDefaultMessagePackSerializerOptions()
    {
        var options = MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create(
                ImmutableCollectionResolver.Instance,
                StandardResolverAllowPrivate.Instance,
                StandardResolver.Instance
            ))
            .WithSecurity(MessagePackSecurity.UntrustedData);
        
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
        _config.SerializerType = _config.SerializerType switch
        {
            // Ensure that a serializer type or stream serializer is specified
            null when _config.StreamSerializer is null => throw new InvalidOperationException(
                "No serializer type or stream serializer specified. Please specify one before building."),
            // Provide a default serializer if none is specified but a stream serializer is set
            null when _config.StreamSerializer is not null => typeof(StreamSerializerAdapter),
            _ => _config.SerializerType
        };

        // Validate specific serializer configurations
        ValidateSerializerConfigurations();

        // Create and return the serializer using the configured settings
        return SerializerFactory.CreateSerializer(_config);
    }

    private void ValidateSerializerConfigurations()
    {
        if (_config.SerializerType == typeof(JsonSerializer) && _config.JsonSerializerOptions is null)
            throw new InvalidOperationException("JsonSerializerOptions must be specified for JsonSerializer.");

        if (_config.SerializerType == typeof(MessagePackSerializer) && _config.MessagePackSerializerOptions is null)
            throw new InvalidOperationException(
                "MessagePackSerializerOptions must be specified for MessagePackSerializer.");
    }
}

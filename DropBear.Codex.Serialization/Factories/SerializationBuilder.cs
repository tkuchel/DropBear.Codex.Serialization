#region

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

#endregion

namespace DropBear.Codex.Serialization.Factories;

/// <summary>
///     Builder class for configuring serialization settings.
/// </summary>
[SupportedOSPlatform("windows")]
public class SerializationBuilder
{
    private readonly SerializationConfig _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SerializationBuilder" /> class.
    /// </summary>
    public SerializationBuilder()
    {
        _config = new SerializationConfig();
    }

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
    ///     Specifies the serialization provider type to use.
    /// </summary>
    /// <param name="serializerType">The type of the serialization provider.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithSerializer(Type serializerType)
    {
        if (serializerType == null)
        {
            throw new ArgumentNullException(nameof(serializerType), "Serializer type cannot be null.");
        }

        if (!typeof(ISerializer).IsAssignableFrom(serializerType))
        {
            throw new ArgumentException("The type must implement the ISerializer interface.", nameof(serializerType));
        }

        _config.SerializerType = serializerType;
        return this;
    }

    /// <summary>
    ///     Specifies the compression provider type to use.
    /// </summary>
    /// <param name="compressionType">The type of the compression provider.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithCompression(Type compressionType)
    {
        if (compressionType == null)
        {
            throw new ArgumentNullException(nameof(compressionType), "Compression provider type cannot be null.");
        }

        if (!typeof(ICompressionProvider).IsAssignableFrom(compressionType))
        {
            throw new ArgumentException("The type must implement the ICompressionProvider interface.",
                nameof(compressionType));
        }

        _config.CompressionProviderType = compressionType;
        return this;
    }

    /// <summary>
    ///     Specifies the encoding provider type to use.
    /// </summary>
    /// <param name="encodingType">The type of the encoding provider.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithEncoding(Type encodingType)
    {
        if (encodingType == null)
        {
            throw new ArgumentNullException(nameof(encodingType), "Encoding provider type cannot be null.");
        }

        if (!typeof(IEncodingProvider).IsAssignableFrom(encodingType))
        {
            throw new ArgumentException("The type must implement the IEncodingProvider interface.",
                nameof(encodingType));
        }

        _config.EncodingProviderType = encodingType;
        return this;
    }

    /// <summary>
    ///     Specifies the encryption provider type to use.
    /// </summary>
    /// <param name="encryptionType">The type of the encryption provider.</param>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithEncryption(Type encryptionType)
    {
        if (encryptionType == null)
        {
            throw new ArgumentNullException(nameof(encryptionType), "Encryption provider type cannot be null.");
        }

        if (!typeof(IEncryptionProvider).IsAssignableFrom(encryptionType))
        {
            throw new ArgumentException("The type must implement the IEncryptionProvider interface.",
                nameof(encryptionType));
        }

        _config.EncryptionProviderType = encryptionType;
        return this;
    }

    /// <summary>
    ///     Specifies the stream serializer instance to use.
    /// </summary>
    /// <typeparam name="T">The type of stream serializer.</typeparam>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithStreamSerializer<T>() where T : IStreamSerializer
    {
        _config.StreamSerializerType = typeof(T);
        return this;
    }

    /// <summary>
    ///     Specifies the compression provider to use.
    /// </summary>
    /// <typeparam name="T">The type of compression provider.</typeparam>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithCompression<T>() where T : ICompressionProvider
    {
        _config.CompressionProviderType = typeof(T);
        return this;
    }

    /// <summary>
    ///     Specifies the encoding provider to use.
    /// </summary>
    /// <typeparam name="T">The type of encoding provider.</typeparam>
    /// <returns>The serialization builder instance.</returns>
    public SerializationBuilder WithEncoding<T>() where T : IEncodingProvider
    {
        _config.EncodingProviderType = typeof(T);
        return this;
    }

    /// <summary>
    ///     Specifies the encryption provider to use by setting the paths to the public and private keys.
    /// </summary>
    /// <param name="publicKeyPath">The path to the public key file.</param>
    /// <param name="privateKeyPath">The path to the private key file.</param>
    /// <returns>The builder with updated key paths, if the keys exist.</returns>
    /// <exception cref="FileNotFoundException">Thrown if either the public or private key file does not exist.</exception>
    public SerializationBuilder WithKeys(string publicKeyPath, string privateKeyPath)
    {
        // Verify that the public key file exists
        if (!File.Exists(publicKeyPath))
        {
            throw new FileNotFoundException($"Public key file not found at path: {publicKeyPath}");
        }

        // Verify that the private key file exists
        if (!File.Exists(privateKeyPath))
        {
            throw new FileNotFoundException($"Private key file not found at path: {privateKeyPath}");
        }

        // Both files exist, update the configuration
        _config.PublicKeyPath = publicKeyPath;
        _config.PrivateKeyPath = privateKeyPath;

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
            Converters =
            {
                new JsonStringEnumConverter() // Use a converter for enums
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
    public SerializationBuilder WithDefaultConfiguration()
    {
        return WithJsonSerializerOptions(new JsonSerializerOptions { WriteIndented = true })
            .WithCompression<GZipCompressionProvider>();
    }

    /// <summary>
    ///     Builds the serializer based on the configured settings.
    /// </summary>
    /// <returns>The configured serializer instance.</returns>
    public ISerializer Build()
    {
        _config.SerializerType = _config.SerializerType switch
        {
            // Ensure that a serializer type or stream serializer is specified
            null when _config.StreamSerializerType is null => throw new InvalidOperationException(
                "No serializer type or stream serializer specified. Please specify one before building."),
            // Provide a default serializer if none is specified but a stream serializer is set
            null when _config.StreamSerializerType is not null => typeof(StreamSerializerAdapter),
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
        {
            throw new InvalidOperationException("JsonSerializerOptions must be specified for JsonSerializer.");
        }

        if (_config.SerializerType == typeof(MessagePackSerializer) && _config.MessagePackSerializerOptions is null)
        {
            throw new InvalidOperationException(
                "MessagePackSerializerOptions must be specified for MessagePackSerializer.");
        }
    }
}

using DropBear.Codex.Serialization.Configurations;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Factories;

public class SerializationBuilder
{
    private readonly SerializationConfig _config;

    public SerializationBuilder()
    {
        _config = new SerializationConfig();
    }

    public SerializationBuilder WithSerializer<T>() where T : ISerializer
    {
        _config.SerializerType = typeof(T);
        return this;
    }

    public SerializationBuilder WithCompression<T>() where T : ICompressionProvider, new()
    {
        _config.CompressionProvider = new T();
        return this;
    }

    public SerializationBuilder WithEncoding<T>() where T : IEncodingProvider, new()
    {
        _config.EncodingProvider = new T();
        return this;
    }

    public SerializationBuilder WithEncryption<T>() where T : IEncryptionProvider, new()
    {
        _config.EncryptionProvider = new T();
        return this;
    }

    public ISerializer Build()
    {
        var serializer = SerializerFactory.CreateSerializer(_config);
        // Apply compression, encoding, and encryption based on the configuration
        return serializer;
    }
}

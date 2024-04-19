using System.Text.Json;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Configurations;

/// <summary>
///     Represents the configuration settings for serialization.
/// </summary>
public class SerializationConfig
{
    /// <summary>
    ///     Gets or sets the type of serializer to be used.
    /// </summary>
    public Type? SerializerType { get; set; }

    /// <summary>
    ///     Gets or sets the compression provider for serialization.
    /// </summary>
    public ICompressionProvider? CompressionProvider { get; set; }

    /// <summary>
    ///     Gets or sets the encoding provider for serialization.
    /// </summary>
    public IEncodingProvider? EncodingProvider { get; set; }

    /// <summary>
    ///     Gets or sets the encryption provider for serialization.
    /// </summary>
    public IEncryptionProvider? EncryptionProvider { get; set; }

    /// <summary>
    ///     Gets or sets the memory stream manager for serialization.
    /// </summary>
    public RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; set; } = new();
    

    /// <summary>
    ///     Gets or sets the stream serializer for serialization.
    /// </summary>
    public IStreamSerializer? StreamSerializer { get; set; }

    /// <summary>
    ///     Gets or sets the JSON serializer options.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    ///     Gets or sets the MessagePack serializer options.
    /// </summary>
    public MessagePackSerializerOptions? MessagePackSerializerOptions { get; set; }
}

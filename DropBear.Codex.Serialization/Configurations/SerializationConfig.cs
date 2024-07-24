#region

using System.Text.Json;
using MessagePack;
using Microsoft.IO;

#endregion

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
    ///     Gets or sets the type of compression provider for serialization.
    /// </summary>
    public Type? CompressionProviderType { get; set; }

    /// <summary>
    ///     Gets or sets the type of encoding provider for serialization.
    /// </summary>
    public Type? EncodingProviderType { get; set; }

    /// <summary>
    ///     Gets or sets the type of encryption provider for serialization.
    /// </summary>
    public Type? EncryptionProviderType { get; set; }

    /// <summary>
    ///     Gets or sets the memory stream manager for serialization.
    /// </summary>
    public RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; set; } = new();


    /// <summary>
    ///     Gets or sets the type of stream serializer for serialization.
    /// </summary>
    public Type? StreamSerializerType { get; set; }

    /// <summary>
    ///     Gets or sets the JSON serializer options.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    ///     Gets or sets the MessagePack serializer options.
    /// </summary>
    public MessagePackSerializerOptions? MessagePackSerializerOptions { get; set; }

    /// <summary>
    ///     Gets or sets the path to the public key file.
    /// </summary>
    public string PublicKeyPath { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the path to the private key file.
    /// </summary>
    public string PrivateKeyPath { get; set; } = string.Empty;
}

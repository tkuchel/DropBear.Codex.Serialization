using System.Text.Json;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;
using Microsoft.IO;

namespace DropBear.Codex.Serialization.Configurations;

public class SerializationConfig
{
    public Type SerializerType { get; set; }
    public ICompressionProvider CompressionProvider { get; set; }
    public IEncodingProvider EncodingProvider { get; set; }
    public IEncryptionProvider EncryptionProvider { get; set; }
    public RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; set; }

    // Add other configuration properties as needed
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
    public MessagePackSerializerOptions MessagePackSerializerOptions { get; set; }
}


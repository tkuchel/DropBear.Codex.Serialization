using System.Text.Json;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Writers;

public class JsonSerializerWriter : ISerializerWriter
{
    private readonly JsonSerializerOptions _options;

    public JsonSerializerWriter(JsonSerializerOptions options) => _options = options;

    public Task SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default) =>
        JsonSerializer.SerializeAsync(stream, value, _options, cancellationToken);
}

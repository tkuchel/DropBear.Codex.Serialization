using System.Text.Json;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Writers;

public class JsonSerializerReader : ISerializerReader
{
    private readonly JsonSerializerOptions _options;

    public JsonSerializerReader(JsonSerializerOptions options) => _options = options;

    public async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default) =>
        await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken).ConfigureAwait(false) ??
        default(T);
}

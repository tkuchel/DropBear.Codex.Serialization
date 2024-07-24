#region

using System.Text.Json;
using DropBear.Codex.Serialization.Interfaces;

#endregion

namespace DropBear.Codex.Serialization.Writers;

/// <summary>
///     Implementation of <see cref="ISerializerWriter" /> for JSON serialization.
/// </summary>
public class JsonSerializerWriter : ISerializerWriter
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonSerializerWriter" /> class with the specified options.
    /// </summary>
    /// <param name="options">The JSON serializer options.</param>
    public JsonSerializerWriter(JsonSerializerOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public Task SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.SerializeAsync(stream, value, _options, cancellationToken);
    }
}

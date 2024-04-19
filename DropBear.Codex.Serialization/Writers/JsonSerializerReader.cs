using System.Text.Json;
using DropBear.Codex.Serialization.Interfaces;

namespace DropBear.Codex.Serialization.Writers
{
    /// <summary>
    /// Implementation of <see cref="ISerializerReader"/> for JSON serialization.
    /// </summary>
    public class JsonSerializerReader : ISerializerReader
    {
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerReader"/> class with the specified options.
        /// </summary>
        /// <param name="options">The JSON serializer options.</param>
        public JsonSerializerReader(JsonSerializerOptions options)
        {
            _options = options;
        }

        /// <inheritdoc/>
        public async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken)
                .ConfigureAwait(false) ?? default;
        }
    }
}

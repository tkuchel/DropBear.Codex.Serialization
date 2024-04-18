namespace DropBear.Codex.Serialization.Interfaces;

public interface ISerializerReader
{
    Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);
}

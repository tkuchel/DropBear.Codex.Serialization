namespace DropBear.Codex.Serialization.Interfaces;

public interface ISerializerWriter
{
    Task SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default);
}

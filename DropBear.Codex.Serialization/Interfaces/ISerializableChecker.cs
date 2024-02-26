using DropBear.Codex.Core.ReturnTypes;

namespace DropBear.Codex.Serialization.Interfaces;

/// <summary>
///     Provides functionality to check if a type is serializable by MessagePack and MemoryPack.
/// </summary>
public interface ISerializableChecker
{
    Result<bool> IsSerializable<T>() where T : class;
}
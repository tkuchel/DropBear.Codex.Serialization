using DropBear.Codex.Core.ReturnTypes;

namespace DropBear.Codex.Serialization.Interfaces;

public interface IMemoryPackSerializableChecker
{
    Result<bool> IsMemoryPackSerializable<T>() where T : class;
}

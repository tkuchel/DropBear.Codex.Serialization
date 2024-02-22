using DropBear.Codex.Core.ReturnTypes;

namespace DropBear.Codex.Serialization.Interfaces;

/// <summary>
///     Provides functionality to check if a type is serializable by MessagePack.
/// </summary>
public interface IMessagePackSerializableChecker
{
    /// <summary>
    ///     Determines whether the specified type is serializable by MessagePack.
    /// </summary>
    /// <typeparam name="T">The type to check for MessagePack serialization compatibility.</typeparam>
    /// <returns>True if the type is serializable by MessagePack, otherwise false.</returns>
    Result<bool> IsMessagePackSerializable<T>() where T : class;
}
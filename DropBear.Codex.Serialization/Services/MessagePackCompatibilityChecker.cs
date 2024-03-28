using System.Collections.Concurrent;
using System.Reflection;
using Cysharp.Text;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Provides utilities for ensuring types are compatible with MessagePack serialization.
/// </summary>
public class MessagePackCompatibilityChecker : ISerializableChecker
{
    // Cache to store compatibility check results to improve performance.
    private readonly ConcurrentDictionary<Type, Result> _compatibilityCache = new();

    public Result IsSerializable<T>() where T : class
    {
        try
        {
            EnsureMessagePackCompatibility<T>();
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure("Type is not compatible with MessagePack serialization.");
        }
    }

    /// <summary>
    ///     Ensures that a type is compatible with MessagePack serialization.
    ///     This includes being public, not nested, having a MessagePackObject attribute,
    ///     and all serializable members having a Key attribute or being marked to ignore.
    /// </summary>
    /// <typeparam name="T">The type to check for MessagePack compatibility.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the type is not compatible with MessagePack serialization.</exception>
    private void EnsureMessagePackCompatibility<T>()
    {
        var type = typeof(T);

        // Check cache first to avoid repeated reflection.
        if (_compatibilityCache.TryGetValue(type, out var result) &&
            result.IsSuccess) return; // Type is already verified as compatible, no need to check again.

        // Ensure the type is public and not nested.
        if (!type.IsPublic || type.IsNested)
            throw new InvalidOperationException(ZString.Format("Type {0} must be public and not nested.", type.Name));

        // Ensure the type has a MessagePackObject attribute.
        var messagePackObjectAttribute = type.GetCustomAttribute<MessagePackObjectAttribute>();
        if (messagePackObjectAttribute is null)
            throw new InvalidOperationException(ZString.Format("Type {0} must have a MessagePackObject attribute.", type.Name));

        // Collect all properties and fields that should be serialized.
        var members = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Cast<MemberInfo>()
            .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance));

        var membersWithoutKey = (from member in members
            let ignoreAttribute = member.GetCustomAttribute<IgnoreMemberAttribute>()
            where ignoreAttribute is null
            let keyAttribute = member.GetCustomAttribute<KeyAttribute>()
            where keyAttribute is null
            select member.Name).ToList();

        // If any members are missing a Key attribute and are not ignored, report them in an exception.
        if (membersWithoutKey.Count is not 0)
            throw new InvalidOperationException(ZString.Format(
                "Type {0} has members without a Key attribute: {1}",
                type.Name,
                string.Join(", ", membersWithoutKey)));

        // Cache the result as the type is compatible.
        _compatibilityCache[type] = Result.Success();
    }
}

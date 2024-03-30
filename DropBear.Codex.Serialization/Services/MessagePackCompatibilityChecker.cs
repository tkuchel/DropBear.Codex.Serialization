using System.Collections.Concurrent;
using System.Reflection;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;
using MessagePack.Formatters;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Provides utilities for ensuring types are compatible with MessagePack serialization.
/// </summary>
public class MessagePackCompatibilityChecker : ISerializableChecker
{
    // Cache to store compatibility check results to improve performance.
    private readonly ConcurrentDictionary<Type, bool> _compatibilityCache = new();

    public Result IsSerializable<T>() where T : class => EnsureMessagePackCompatibility<T>()
        ? Result.Success()
        : Result.Failure("Type is not compatible with MessagePack serialization.");

    /// <summary>
    ///     Ensures that a type is compatible with MessagePack serialization.
    ///     This includes being public, not nested, having a MessagePackObject attribute,
    ///     and all serializable members having a Key attribute or being marked to ignore.
    /// </summary>
    /// <typeparam name="T">The type to check for MessagePack compatibility.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the type is not compatible with MessagePack serialization.</exception>
    private bool EnsureMessagePackCompatibility<T>()
    {
        try
        {
            var type = typeof(T);
            if (_compatibilityCache.TryGetValue(type, out var isCompatible) && isCompatible) return true;

            if (!type.IsPublic || type.IsNested)
                throw new InvalidOperationException($"Type {type.Name} must be public and not nested.");

            var messagePackObjectAttribute = type.GetCustomAttribute<MessagePackObjectAttribute>();
            if (messagePackObjectAttribute is null)
                throw new InvalidOperationException($"Type {type.Name} must have a MessagePackObject attribute.");

            EnsureHasSerializationConstructor(type);

            var members = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Cast<MemberInfo>()
                .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance)).ToList();

            EnsureMembersHaveKeyOrIgnore(members, type);

            // Additional check for string properties that might need the StringInterningFormatter
            // This part is highly specific and may need adjustment based on the application's needs.
            EnsureStringPropertiesFormattedCorrectly(type);

            _compatibilityCache[type] = true;
            return true;
        }
        catch (Exception)
        {
            _compatibilityCache[typeof(T)] = false;
            return false;
        }
    }

    private static void EnsureHasSerializationConstructor(Type type)
    {
        var hasSerializationConstructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Any(c => c.GetCustomAttribute<SerializationConstructorAttribute>() != null);

        if (!hasSerializationConstructor)
            throw new InvalidOperationException(
                $"Type {type.Name} must have at least one constructor with the SerializationConstructor attribute.");
    }

    private static void EnsureMembersHaveKeyOrIgnore(IEnumerable<MemberInfo> members, MemberInfo type)
    {
        var membersWithoutKey = members
            .Where(m => m.GetCustomAttribute<IgnoreMemberAttribute>() == null &&
                        m.GetCustomAttribute<KeyAttribute>() == null)
            .Select(m => m.Name)
            .ToList();

        if (membersWithoutKey.Count != 0)
            throw new InvalidOperationException(
                $"The following members in type {type.Name} must have a Key attribute for MessagePack serialization or be marked with IgnoreMember: {string.Join(", ", membersWithoutKey)}.");
    }

    private static void EnsureStringPropertiesFormattedCorrectly(Type type)
    {
        // Assuming string properties that require interning end with "Interned"
        var stringProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(pi => pi.PropertyType == typeof(string) && pi.Name.EndsWith("Interned", StringComparison.OrdinalIgnoreCase));

        foreach (var prop in stringProperties)
        {
            var formatterAttribute = prop.GetCustomAttribute<MessagePackFormatterAttribute>();
            if (formatterAttribute?.FormatterType != typeof(StringInterningFormatter))
                throw new InvalidOperationException(
                    $"Property {prop.Name} in type {type.Name} is expected to use StringInterningFormatter for MessagePack serialization.");
        }
    }
}

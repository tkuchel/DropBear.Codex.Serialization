using System.Collections.Concurrent;
using System.Reflection;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Interfaces;
using MemoryPack;

namespace DropBear.Codex.Serialization.Services;

public class MemoryPackSerializableChecker : ISerializableChecker
{
    private static readonly ConcurrentDictionary<Type, Result<bool>> Cache = new();

    public Result<bool> IsSerializable<T>() where T : class
    {
        var type = typeof(T);

        // Return cached result if available
        if (Cache.TryGetValue(type, out var cachedResult)) return cachedResult;

        // Check if type is public
        if (!type.IsPublic)
            return CacheAndReturn(type, Result<bool>.Failure($"Type '{type.Name}' must be public."));

        // Check for MemoryPackable attribute
        var attribute = type.GetCustomAttribute<MemoryPackableAttribute>();
        return CacheAndReturn(type, attribute is null ? Result<bool>.Failure($"Type '{type.Name}' lacks MemoryPackableAttribute.") : Result<bool>.Success(value: true));
    }

    private static Result<bool> CacheAndReturn(Type type, Result<bool> result)
    {
        Cache[type] = result;
        return result;
    }
}

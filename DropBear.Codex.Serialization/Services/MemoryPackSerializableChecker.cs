using System.Collections.Concurrent;
using System.Reflection;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Interfaces;
using MemoryPack;

namespace DropBear.Codex.Serialization.Services;

public class MemoryPackSerializableChecker : ISerializableChecker
{
    private static readonly ConcurrentDictionary<Type, Result<bool>> _cache = new();

    public Result<bool> IsSerializable<T>() where T : class
    {
        var type = typeof(T);

        // Return cached result if available
        if (_cache.TryGetValue(type, out var cachedResult)) return cachedResult;

        // Check if type is public
        if (!type.IsPublic)
            return CacheAndReturn(type, Result<bool>.Failure($"Type '{type.Name}' must be public."));

        // Check for MemoryPackable attribute
        var attribute = type.GetCustomAttribute<MemoryPackableAttribute>();
        if (attribute == null)
            return CacheAndReturn(type, Result<bool>.Failure($"Type '{type.Name}' lacks MemoryPackableAttribute."));

        // Since we cannot directly check if a class is partial, this is a limitation and should be documented.

        return CacheAndReturn(type, Result<bool>.Success(true));
    }

    private Result<bool> CacheAndReturn(Type type, Result<bool> result)
    {
        _cache[type] = result;
        return result;
    }
}

public interface IMemoryPackSerializableChecker
{
    Result<bool> IsMemoryPackSerializable<T>() where T : class;
}
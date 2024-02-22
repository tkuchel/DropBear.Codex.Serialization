﻿using System.Collections.Concurrent;
using System.Reflection;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Interfaces;
using MessagePack;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Checks for MessagePack serializability of types based on MessagePack attributes.
/// </summary>
public class MessagePackSerializableChecker : IMessagePackSerializableChecker
{
    private static readonly ConcurrentDictionary<Type, Result<bool>> _cache = new();

    /// <summary>
    ///     Determines if a type is serializable by MessagePack by inspecting its attributes.
    ///     Results are cached to improve performance on subsequent checks.
    /// </summary>
    /// <typeparam name="T">The type to check for MessagePack serializability.</typeparam>
    /// <returns>
    ///     A result indicating whether the type is serializable by MessagePack.
    ///     Success with true if serializable, otherwise Failure with a message.
    /// </returns>
    public Result<bool> IsMessagePackSerializable<T>() where T : class
    {
        var type = typeof(T);

        // Return cached result if available
        if (_cache.TryGetValue(type, out var cachedResult)) return cachedResult;

        var attribute = type.GetCustomAttribute<MessagePackObjectAttribute>();
        if (attribute == null)
            return CacheAndReturn(type, Result<bool>.Failure($"Type '{type.Name}' lacks MessagePackObjectAttribute."));

        if (attribute.KeyAsPropertyName) return CacheAndReturn(type, Result<bool>.Success(true));

        var allPropertiesHaveKey = type.GetProperties().All(p => p.GetCustomAttributes<KeyAttribute>().Any());
        return CacheAndReturn(type,
            allPropertiesHaveKey
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Not all properties on '{type.Name}' have KeyAttribute."));
    }

    /// <summary>
    ///     Caches the result of the serializability check and returns the result.
    /// </summary>
    /// <param name="type">The type that was checked.</param>
    /// <param name="result">The result of the check.</param>
    /// <returns>The result of the serializability check.</returns>
    private Result<bool> CacheAndReturn(Type type, Result<bool> result)
    {
        _cache[type] = result;
        return result;
    }
}
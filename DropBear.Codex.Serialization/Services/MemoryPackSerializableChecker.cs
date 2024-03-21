using System.Collections.Concurrent;
using System.Reflection;
using Cysharp.Text;
using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.Serialization.Interfaces;
using MemoryPack;

namespace DropBear.Codex.Serialization.Services;

/// <summary>
///     Checks if a type is serializable with MemoryPack.
/// </summary>
public class MemoryPackSerializableChecker : ISerializableChecker
{
    private static readonly ConcurrentDictionary<Type, Result<bool>> Cache = new();


    /// <summary>
    ///     Checks if the specified type is serializable with MemoryPack.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns>A <see cref="Result{Boolean}" /> indicating if the type is serializable.</returns>
    public Result<bool> IsSerializable<T>() where T : class
    {
        var type = typeof(T);

        // Check cache for cached result
        if (Cache.TryGetValue(type, out var cachedResult))
            return cachedResult;

        try
        {
            // Check if type is public
            if (!type.IsPublic)
                return CacheAndReturn(type, Result<bool>.Failure("Type is not public."));

            // Check for MemoryPackable attribute
            var attribute = type.GetCustomAttribute<MemoryPackableAttribute>();
            return CacheAndReturn(type, attribute is null
                ? Result<bool>.Failure(ZString.Format("Type '{0}' lacks MemoryPackableAttribute.", type.Name))
                : Result<bool>.Success(value: true));
        }
        catch (Exception ex)
        {
            // Log or handle the exception gracefully
            return CacheAndReturn(type, Result<bool>.Failure(ZString.Format("MemoryPackable check failed: {0}", ex.Message)));
        }
    }

    private static Result<bool> CacheAndReturn(Type type, Result<bool> result)
    {
        Cache[type] = result;
        return result;
    }
}

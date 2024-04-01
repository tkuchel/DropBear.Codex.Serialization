using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessagePack;

namespace DropBear.Codex.Serialization.Services
{
    public class MessagePackCompatibilityChecker
    {
        private readonly ConcurrentDictionary<Type, bool> _compatibilityCache = new();

        /// <summary>
        /// Checks if the given type is compatible with MessagePack serialization.
        /// </summary>
        /// <param name="type">The type to check for MessagePack compatibility.</param>
        /// <returns>True if the type is compatible with MessagePack serialization, otherwise false.</returns>
        public bool IsSerializable(Type type)
        {
            // Return cached result if available
            if (_compatibilityCache.TryGetValue(type, out var isCompatible))
            {
                return isCompatible;
            }

            try
            {
                // Perform compatibility checks
                isCompatible = PerformCompatibilityChecks(type);

                // Cache the result
                _compatibilityCache[type] = isCompatible;
            }
            catch
            {
                // Consider logging the exception or handling it as needed
                // Cache the negative result
                _compatibilityCache[type] = false;
                isCompatible = false;
            }

            return isCompatible;
        }

        private bool PerformCompatibilityChecks(Type type)
        {
            if (!type.IsPublic || type.IsNested)
            {
                return false;
            }

            if (type.GetCustomAttribute<MessagePackObjectAttribute>() is null)
            {
                return false;
            }

            var members = GetAllSerializableMembers(type);
            if (members.Any(member => member.GetCustomAttribute<KeyAttribute>() is null && member.GetCustomAttribute<IgnoreMemberAttribute>() is null))
            {
                return false;
            }

            if (type.IsInterface || type.IsAbstract)
            {
                return EnsureValidUnionAttributes(type);
            }

            return true;
        }

        private static IEnumerable<MemberInfo> GetAllSerializableMembers(Type type) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Cast<MemberInfo>()
                .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance));

        private static bool EnsureValidUnionAttributes(Type type)
        {
            var unionAttributes = type.GetCustomAttributes<UnionAttribute>().ToList();
            if (unionAttributes.Count == 0 || unionAttributes.Select(attr => attr.Key).Distinct().Count() != unionAttributes.Count)
            {
                return false;
            }

            foreach (var attr in unionAttributes)
            {
                if (!attr.SubType.IsSubclassOf(type) && !type.IsAssignableFrom(attr.SubType))
                {
                    return false;
                }
                if (attr.SubType.GetCustomAttribute<MessagePackObjectAttribute>() is null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

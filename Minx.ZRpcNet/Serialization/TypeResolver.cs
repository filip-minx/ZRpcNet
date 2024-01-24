using System;
using System.Collections.Generic;
using System.Linq;

namespace Minx.ZRpcNet.Serialization
{
    internal static class TypeResolver
    {
        private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        internal static Type GetTypeInAllAssemblies(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));

            if (typeCache.TryGetValue(typeName, out Type cachedType))
                return cachedType;

            var type = Type.GetType(typeName);

            if (type == null)
            {
                type = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Equals(typeName, StringComparison.Ordinal));
            }

            typeCache[typeName] = type;

            return type;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Minx.ZRpcNet.Serialization
{
    public static class TypeResolver
    {
        private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        public static Type GetTypeInAllAssemblies(string typeName, string hintAssemblyName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));

            if (typeCache.TryGetValue(typeName, out Type cachedType))
                return cachedType;

            Type type = null;

            if (hintAssemblyName != null)
            {
                var tn = typeName + ", " + hintAssemblyName;
                type = Type.GetType(tn);
            }

            if (type == null)
            {
                type = Type.GetType(typeName);
            }

            typeCache[typeName] = type;

            if (type == null)
            {
                throw new InvalidOperationException($"Could not resolve type. TypeName: {typeName}, AssemblyName: {hintAssemblyName}");
            }
            return type;
        }

        public static Type GetTypeInAllAssemblies(TypeLocator typeLocator)
        {
            return GetTypeInAllAssemblies(typeLocator.TypeName, typeLocator.AssemblyName);
        }
    }
}

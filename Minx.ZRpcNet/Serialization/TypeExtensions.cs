using Minx.ZRpcNet.Serialization;
using System;

namespace Minx.ZRpcNet
{
    public static class TypeExtensions
    {
        public static TypeLocator GetTypeLocator(this Type type)
        {
            return new TypeLocator()
            {
                TypeName = type.FullName,
                AssemblyName = type.Assembly.FullName
            };
        }
    }
}

using Newtonsoft.Json.Serialization;
using System;

namespace Minx.ZRpcNet.Serialization
{
    public class CustomSerializationBinder : DefaultSerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return TypeResolver.GetTypeInAllAssemblies(typeName, assemblyName);
        }
    }
}

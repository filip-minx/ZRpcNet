using Newtonsoft.Json;

namespace Minx.ZRpcNet.Serialization
{
    public static class MessageSerializationSettings
    {
        public static readonly JsonSerializerSettings Instance = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };
    }
}

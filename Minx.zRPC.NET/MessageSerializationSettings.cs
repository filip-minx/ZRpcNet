using System.Collections.Generic;
using Newtonsoft.Json;

namespace Minx.zRPC.NET
{
    public static class MessageSerializationSettings
    {
        public static readonly JsonSerializerSettings Instance = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = new List<JsonConverter>()
            {
                new Int32Converter()
            }
        };
    }
}

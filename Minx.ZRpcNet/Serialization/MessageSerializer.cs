using Newtonsoft.Json;

namespace Minx.ZRpcNet.Serialization
{
    public class MessageSerializer
    {
        public static string SerializeMessage(object message)
        {
            return JsonConvert.SerializeObject(message, MessageSerializationSettings.Instance);
        }

        public static T DeserializeMessage<T>(string json)
        {
            if (typeof(T) == typeof(InvocationMessage))
            {
                return (T)(object)InvocationMessageSerializer.DeserializeInvocation(json);
            }

            return JsonConvert.DeserializeObject<T>(json, MessageSerializationSettings.Instance);
        }
    }

}
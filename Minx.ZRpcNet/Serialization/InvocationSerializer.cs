using Newtonsoft.Json;
using System;
using System.Linq;

namespace Minx.ZRpcNet.Serialization
{
    internal static class InvocationSerializer
    {
        public static string SerializeInvocation(InvocationMessage message)
        {
            // Enum values need to be boxed in the arguments array since it is 
            // an array of generic objects. Otherwise the JsonConvert class will
            // serialize this without the specific Enum type. This is needed for
            // deserialization of the Enum to the original type.
            // This has been easier to implement than a custom JSON converter
            // that would handle this. Ideally, this should be replaced eventually.

            var replacedArguments = message.Arguments
                .Select(a =>
                    a is Enum
                        ? new EnumBox
                        {
                            Value = a.ToString(),
                            Type = a.GetType().AssemblyQualifiedName
                        }
                        : a);

            var replacedMessage = new InvocationMessage()
            {
                MethodName = message.MethodName,
                TypeName = message.TypeName,
                Arguments = replacedArguments.ToArray()
            };

            return JsonConvert.SerializeObject(replacedMessage, MessageSerializationSettings.Instance);
        }

        public static InvocationMessage DeserializeInvocation(string json)
        {
            var invocation = JsonConvert.DeserializeObject<InvocationMessage>(json, MessageSerializationSettings.Instance);

            for (int i = 0; i < invocation.Arguments.Length; i++)
            {
                if (invocation.Arguments[i] is EnumBox enumBox)
                {
                    invocation.Arguments[i] = Enum.Parse(Type.GetType(enumBox.Type), enumBox.Value);
                }
            }

            return invocation;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Linq;

namespace Minx.ZRpcNet.Serialization
{
    internal static class InvocationMessageSerializer
    {
        public static InvocationMessage DeserializeInvocation(string json)
        {
            var invocation = JsonConvert.DeserializeObject<InvocationMessage>(json, MessageSerializationSettings.Instance);

            invocation.Arguments = ConvertArgumentTypes(
                invocation.Arguments,
                invocation.ArgumentsTypeNames.Select(typeName => Type.GetType(typeName)).ToArray());

            return invocation;
        }

        public static InvocationResult DeserializeInvocationResult(string json)
        {
            var result = JsonConvert.DeserializeObject<InvocationResult>(json, MessageSerializationSettings.Instance);

            // ResultTypeName is null when the invocation results in an exception.
            if (result.ResultTypeName == null)
            {
                return result;
            }

            // Newtonsoft JSON serializes all value types without their specific .NET types.
            // Convert the result to the correct type if it is a value type.
            // Except for void since the result is always null in that case.

            var resultType = Type.GetType(result.ResultTypeName);

            if (resultType.IsValueType && resultType != typeof(void))
            {
                result.Result = Convert.ChangeType(result.Result, Type.GetType(result.ResultTypeName));
            }

            return result;
        }

        private static object[] ConvertArgumentTypes(object[] arguments, Type[] targetTypes)
        {
            if (arguments?.Length == 0)
            {
                return arguments;
            }

            // Newtonsoft JSON serializes all value types without their specific .NET types.
            // All the arguments need to be converted to their respective types as they might differ from the target types
            // after deserialization.

            var converted = new object[arguments.Length];

            for (int i = 0; i < arguments.Length; i++)
            {
                if (targetTypes[i].IsValueType && arguments[i].GetType() != targetTypes[i])
                {
                    converted[i] = targetTypes[i].IsEnum
                        ? Enum.Parse(targetTypes[i], arguments[i].ToString())
                        : Convert.ChangeType(arguments[i], targetTypes[i]);
                }
                else
                {
                    converted[i] = arguments[i];
                }
            }

            return converted;
        }
    }
}

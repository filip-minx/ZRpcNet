using Castle.DynamicProxy;
using Minx.ZRpcNet.Serialization;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Linq;
using System.Reflection;

namespace Minx.ZRpcNet
{
    public class InvocationInterceptor : IInterceptor
    {
        private readonly Type interceptedType;
        private readonly string requestConnectionString;
        private readonly ZRpcClientOptions options;

        public InvocationInterceptor(Type interceptedType, string requestConnectionString, ZRpcClientOptions options)
        {
            this.interceptedType = interceptedType;
            this.requestConnectionString = requestConnectionString;
            this.options = options;
        }
        private bool IsEventAccessor(MethodInfo method)
        {
            return method.IsSpecialName && (method.Name.StartsWith("add_") || method.Name.StartsWith("remove_"));
        }

        public void Intercept(IInvocation invocation)
        {
            if (IsEventAccessor(invocation.Method))
            {
                invocation.Proceed();
                return;
            }

            var procedureInvocation = new InvocationMessage
            {
                TypeName = interceptedType.FullName,
                MethodName = invocation.Method.Name,
                Arguments = invocation.Arguments,
                ArgumentsTypeNames = GetMethodParametersTypeNames(invocation.Method)
            };

            var requestJson = MessageSerializer.SerializeMessage(procedureInvocation);

            using (var requestSocket = new RequestSocket(requestConnectionString))
            {
                requestSocket.SendFrame(requestJson);

                if (!requestSocket.TryReceiveFrameString(options.Timeout, out var responseJson))
                {
                    throw new TimeoutException();
                }

                var result = MessageSerializer.DeserializeMessage<InvocationResult>(responseJson);

                if (result.Exception != null)
                {
                    throw result.Exception;
                }

                invocation.ReturnValue = result.Result;
            }
        }

        private static string[] GetMethodParametersTypeNames(MethodInfo method)
        {
            return method.GetParameters().Select(p => p.ParameterType.AssemblyQualifiedName).ToArray();
        }
    }
}

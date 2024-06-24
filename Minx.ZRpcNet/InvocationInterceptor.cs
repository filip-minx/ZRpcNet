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
                Type = interceptedType.GetTypeLocator(),
                MethodName = invocation.Method.Name,
                Arguments = invocation.Arguments,
                ArgumentsTypes = GetMethodParametersTypeNames(invocation.Method)
            };

            var requestJson = MessageSerializer.SerializeMessage(procedureInvocation);

            using (var requestSocket = new RequestSocket(requestConnectionString))
            {
                TryConfigureEncryption(requestSocket, options);

                requestSocket.SendFrame(requestJson);

                if (!requestSocket.TryReceiveFrameString(options.GetTimeout(interceptedType), out var responseJson))
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

        private static TypeLocator[] GetMethodParametersTypeNames(MethodInfo method)
        {
            return method.GetParameters().Select(p => p.ParameterType.GetTypeLocator()).ToArray();
        }

        private void TryConfigureEncryption(RequestSocket requestSocket, ZRpcClientOptions options)
        {
            if (options.CurveServerPublicKey != null)
            {
                var clientPair = new NetMQCertificate();
                requestSocket.Options.CurveServerKey = options.CurveServerPublicKey;
                requestSocket.Options.CurveCertificate = clientPair;
            }
        }
    }
}

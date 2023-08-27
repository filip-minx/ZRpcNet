using Castle.DynamicProxy;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;

namespace Minx.zRPC.NET
{
    public class InvocationInterceptor : IInterceptor
    {
        private readonly Type interceptedType;
        private readonly string requestConnectionString;

        public InvocationInterceptor(Type interceptedType, string requestConnectionString)
        {
            this.interceptedType = interceptedType;
            this.requestConnectionString = requestConnectionString;
        }
        private bool IsEventAccessor(System.Reflection.MethodInfo method)
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
                Arguments = invocation.Arguments
            };

            var requestJson = JsonConvert.SerializeObject(procedureInvocation, MessageSerializationSettings.Instance);

            using (var requestSocket = new RequestSocket(requestConnectionString))
            {
                requestSocket.SendFrame(requestJson);

                var responseJson = requestSocket.ReceiveFrameString();

                var result = JsonConvert.DeserializeObject<InvocationResult>(responseJson, MessageSerializationSettings.Instance);

                invocation.ReturnValue = result.Result;
            }
        }
    }
}

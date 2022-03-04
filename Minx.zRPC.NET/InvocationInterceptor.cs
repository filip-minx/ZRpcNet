using Castle.DynamicProxy;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Minx.zRPC.NET
{
    public class InvocationInterceptor : IInterceptor
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = new List<JsonConverter>()
            {
                new Int32Converter()
            }
        };

        private readonly Type interceptedType;
        private readonly RequestSocket socket;

        public InvocationInterceptor(Type interceptedType, RequestSocket socket)
        {
            this.interceptedType = interceptedType;
            this.socket = socket;
        }

        public void Intercept(IInvocation invocation)
        {
            var procedureInvocation = new Invocation
            {
                TypeName = interceptedType.FullName,
                MethodName = invocation.Method.Name,
                Arguments = invocation.Arguments
            };

            var requestJson = JsonConvert.SerializeObject(procedureInvocation, SerializerSettings);

            socket.SendFrame(requestJson);

            var responseJson = socket.ReceiveFrameString();

            var result = JsonConvert.DeserializeObject<InvocationResult>(responseJson, SerializerSettings);

            invocation.ReturnValue = result.Result;
        }
    }
}

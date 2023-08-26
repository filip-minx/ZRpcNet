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
        private readonly string requestConnectionString;

        public InvocationInterceptor(Type interceptedType, string requestConnectionString)
        {
            this.interceptedType = interceptedType;
            this.requestConnectionString = requestConnectionString;
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

            using (var requestSocket = new RequestSocket(requestConnectionString))
            {
                requestSocket.SendFrame(requestJson);

                var responseJson = requestSocket.ReceiveFrameString();

                var result = JsonConvert.DeserializeObject<InvocationResult>(responseJson, SerializerSettings);

                invocation.ReturnValue = result.Result;
            }
        }
    }
}

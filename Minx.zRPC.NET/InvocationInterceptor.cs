using Castle.DynamicProxy;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Minx.zRPC.NET
{
    public class InvocationInterceptor<T> : IInterceptor
    {
        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private RequestSocket socket;

        public InvocationInterceptor(RequestSocket socket)
        {
            this.socket = socket;
        }

        public void Intercept(IInvocation invocation)
        {
            var procedureInvocation = new Invocation
            {
                TypeName = typeof(T).FullName,
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

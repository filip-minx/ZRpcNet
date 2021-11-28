using Castle.DynamicProxy;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace NetMQServer
{
    public class InvocationInterceptor<T> : IInterceptor
    {
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

            var requestJson = JsonConvert.SerializeObject(procedureInvocation, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            socket.SendFrame(requestJson);

            var responseJson = socket.ReceiveFrameString();

            var result = JsonConvert.DeserializeObject<InvocationResult>(responseJson, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            invocation.ReturnValue = result.Result;
        }
    }
}

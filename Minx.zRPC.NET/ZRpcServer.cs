using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Minx.zRPC.NET
{
    public class ZRpcServer : IDisposable
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            Converters = new List<JsonConverter>()
            {
                new Int32Converter()
            }
        };

        private Dictionary<string, object> services = new Dictionary<string, object>();

        private NetMQPoller poller;

        private ResponseSocket responseSocket;
        private PublisherSocket publisherSocket;

        public ZRpcServer(string address, int responsePort, int eventPort)
        {
            responseSocket = new ResponseSocket($"@tcp://{address}:{responsePort}");
            publisherSocket = new PublisherSocket($"@tcp://{address}:{eventPort}");

            responseSocket.ReceiveReady += HandleProcedureInvocationRequest;

            poller = new NetMQPoller()
            {
                responseSocket
            };

            poller.RunAsync();
        }

        private void HandleProcedureInvocationRequest(object sender, NetMQSocketEventArgs e)
        {
            var invocationJson = e.Socket.ReceiveFrameString();
            
            var invocation = JsonConvert.DeserializeObject<Invocation>(invocationJson, SerializerSettings);

            var result = Invoke(invocation);

            var resultJson = JsonConvert.SerializeObject(result, SerializerSettings);

            e.Socket.SendFrame(resultJson);
        }

        public void RegisterService<TInterface, TImplementation>(TImplementation implementation)
            where TImplementation : class, TInterface
        {
            services.Add(typeof(TInterface).FullName, implementation);

            EventInterceptor.Create(implementation, SendEvent);
        }

        public InvocationResult Invoke(Invocation invocation)
        {
            var service = services[invocation.TypeName];

            var argumentsTypes = invocation.Arguments
                .Select(a => a.GetType())
                .ToArray();

            var methodInfo = service
                .GetType()
                .GetMethod(invocation.MethodName, argumentsTypes);

            var result = methodInfo.Invoke(service, invocation.Arguments);

            return new InvocationResult()
            {
                Result = result,
                Invocation = invocation
            };
        }

        private void SendEvent(object[] args)
        {
            var eventArgsJson = JsonConvert.SerializeObject(args, SerializerSettings);

            var st = new StackTrace();

            publisherSocket.SendFrame(eventArgsJson);
        }

        public void Dispose()
        {
            poller?.Dispose();
            poller = null;

            responseSocket?.Dispose();
            responseSocket = null;
        }
    }
}

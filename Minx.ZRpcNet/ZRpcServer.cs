using Minx.ZRpcNet.Serialization;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minx.ZRpcNet
{
    public class ZRpcServer : IZRpcServer, IDisposable
    {
        public const int DefaultRequestPort = 28555;
        public const int DefaultEventPort = 28556;

        private readonly Dictionary<string, object> services = new Dictionary<string, object>();

        private NetMQPoller poller;

        private ResponseSocket responseSocket;
        private PublisherSocket publisherSocket;

        public ZRpcServer(string address, int responsePort = DefaultRequestPort, int eventPort = DefaultEventPort)
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

        public void RegisterService<TInterface, TImplementation>(TImplementation implementation)
            where TImplementation : class, TInterface
        {
            services.Add(typeof(TInterface).FullName, implementation);

            EventInterceptor.CreateForAllEvents(typeof(TInterface), implementation, SendEvent);
        }

        private void HandleProcedureInvocationRequest(object sender, NetMQSocketEventArgs e)
        {
            var invocationJson = e.Socket.ReceiveFrameString();

            var invocation = MessageSerializer.DeserializeMessage<InvocationMessage>(invocationJson);

            var result = Invoke(invocation);

            var resultJson = MessageSerializer.SerializeMessage(result);

            e.Socket.SendFrame(resultJson);
        }

        private InvocationResult Invoke(InvocationMessage invocation)
        {
            if (!services.TryGetValue(invocation.TypeName, out object service))
            {
                return new InvocationResult()
                {
                    Exception = new ZRpcServiceNotRegisteredException($"Service '{invocation.TypeName}' is not registered.")
                };
            }

            var argumentsTypes = invocation.ArgumentsTypeNames
                .Select(Type.GetType)
                .ToArray();

            var methodInfo = service
                .GetType()
                .GetMethod(invocation.MethodName, argumentsTypes);

            try
            {
                var result = methodInfo.Invoke(service, invocation.Arguments);

                return new InvocationResult()
                {
                    Result = result,
                    ResultTypeName = methodInfo.ReturnType.AssemblyQualifiedName
                };
            }
            catch (TargetInvocationException ex)
            {
                return new InvocationResult()
                {
                    Exception = new ZRpcInvocationException($"Invocation of '{invocation.TypeName}.{invocation.MethodName}' has failed.", ex.InnerException)
                };
            }
        }

        private void SendEvent(Type interceptedType, EventInfo eventInfo, object[] args)
        {
            var eventData = new EventMessage()
            {
                EventArgs = args[1],
                EventName = eventInfo.Name,
                TypeName = interceptedType.FullName
            };

            var eventArgsJson = MessageSerializer.SerializeMessage(eventData);

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

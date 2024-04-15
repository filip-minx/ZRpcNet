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

        internal ResponseSocket responseSocket;
        internal PublisherSocket publisherSocket;
        internal NetMQQueue<string> messageQueue;

        public NetMQCertificate ServerPair { get; private set; }

        public ZRpcServer(string address, int responsePort = DefaultRequestPort, int eventPort = DefaultEventPort)
        {
            responseSocket = new ResponseSocket($"@tcp://{address}:{responsePort}");
            publisherSocket = new PublisherSocket($"@tcp://{address}:{eventPort}");
            messageQueue = new NetMQQueue<string>();

            ServerPair = new NetMQCertificate();

            responseSocket.ReceiveReady += HandleProcedureInvocationRequest;
            messageQueue.ReceiveReady += HandleMessageReady;

            poller = new NetMQPoller()
            {
                responseSocket,
                messageQueue
            };

            poller.RunAsync();
        }

        public void RegisterService<TInterface, TImplementation>(TImplementation implementation)
            where TImplementation : class, TInterface
        {
            services[typeof(TInterface).FullName] = implementation;

            EventInterceptor.CreateForAllEvents(typeof(TInterface), implementation, SendEvent);
        }

        public void UnregisterService<TInterface>()
        {
            services.Remove(typeof(TInterface).FullName);
        }

        public bool IsServiceRegistered<TInterface>()
        {
            return services.ContainsKey(typeof(TInterface).FullName);
        }

        private void HandleProcedureInvocationRequest(object sender, NetMQSocketEventArgs e)
        {
            var invocationJson = e.Socket.ReceiveFrameString();

            var invocation = MessageSerializer.DeserializeMessage<InvocationMessage>(invocationJson);

            var result = Invoke(invocation);

            var resultJson = MessageSerializer.SerializeMessage(result);

            e.Socket.SendFrame(resultJson);
        }

        private void HandleMessageReady(object sender, NetMQQueueEventArgs<string> e)
        {
            while (e.Queue.TryDequeue(out string message, TimeSpan.Zero))
            {
                publisherSocket.SendFrame(message);
            }
        }

        private InvocationResult Invoke(InvocationMessage invocation)
        {
            if (!services.TryGetValue(invocation.Type.TypeName, out object service))
            {
                return new InvocationResult()
                {
                    Exception = new ZRpcServiceNotRegisteredException($"Service '{invocation.Type}' is not registered.")
                };
            }

            var argumentsTypes = invocation.ArgumentsTypes
                .Select(TypeResolver.GetTypeInAllAssemblies)
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
                    ResultType = methodInfo.ReturnType.GetTypeLocator()
                };
            }
            catch (TargetInvocationException ex)
            {
                return new InvocationResult()
                {
                    Exception = new ZRpcInvocationException($"Invocation of '{invocation.Type}.{invocation.MethodName}' has failed.", ex.InnerException)
                };
            }
        }

        private void SendEvent(Type interceptedType, EventInfo eventInfo, object[] args)
        {
            var eventData = new EventMessage()
            {
                EventArgs = args[1],
                EventName = eventInfo.Name,
                Type = interceptedType.GetTypeLocator()
            };

            var eventArgsJson = MessageSerializer.SerializeMessage(eventData);
            messageQueue.Enqueue(eventArgsJson);
        }

        public void Dispose()
        {
            poller?.Dispose();
            poller = null;

            responseSocket?.Dispose();
            responseSocket = null;

            publisherSocket?.Dispose();
            publisherSocket = null;

            messageQueue?.Dispose();
            messageQueue = null;
        }
    }
}

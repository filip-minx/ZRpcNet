using Castle.DynamicProxy;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Minx.zRPC.NET
{
    public class ZRpcClient : IDisposable
    {
        private Dictionary<Type, object> ProxyInstances = new Dictionary<Type, object>();


        private static ProxyGenerator ProxyGenerator = new ProxyGenerator();

        private string requestConnectionString;
        private string eventConnectionString;

        private SubscriberSocket subscriberSocket;
        private NetMQPoller poller;

        public ZRpcClient(string address, int requestPort, int eventPort)
        {
            requestConnectionString = $">tcp://{address}:{requestPort}";
            eventConnectionString = $">tcp://{address}:{eventPort}";

            subscriberSocket = new SubscriberSocket(eventConnectionString);
            subscriberSocket.SubscribeToAnyTopic();
            subscriberSocket.ReceiveReady += HandleEvent;

            poller = new NetMQPoller()
            {
                subscriberSocket
            };

            poller.RunAsync();
        }

        public T GetService<T>() where T : class
        {
            var interceptor = new InvocationInterceptor(typeof(T), requestConnectionString);

            var proxy = (T)ProxyGenerator.CreateInterfaceProxyWithoutTarget(
                typeof(T),
                interceptor);

            ProxyInstances.Add(typeof(T), proxy);

            return proxy;
        }

        private void HandleEvent(object sender, NetMQSocketEventArgs e)
        {
            var eventJson = e.Socket.ReceiveFrameString();
            Console.WriteLine(eventJson);
        }

        public void Dispose()
        {
            subscriberSocket?.Dispose();
            subscriberSocket = null;

            poller?.Dispose();
            poller = null;
        }
    }
}

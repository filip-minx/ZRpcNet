using Castle.DynamicProxy;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Minx.ZRpcNet
{
    public class ZRpcClient : IDisposable
    {
        private readonly Dictionary<string, object> targetInstances = new Dictionary<string, object>();

        private static ProxyGenerator ProxyGenerator = new ProxyGenerator();

        private string requestConnectionString;
        private string eventConnectionString;

        private SubscriberSocket subscriberSocket;
        private NetMQPoller poller;

        public ZRpcClient(string address, int requestPort = ZRpcServer.DefaultRequestPort, int eventPort = ZRpcServer.DefaultEventPort)
        {
            requestConnectionString = $">tcp://{address}:{requestPort}";
            eventConnectionString = $">tcp://{address}:{eventPort}";

            subscriberSocket = new SubscriberSocket(eventConnectionString);
            subscriberSocket.SubscribeToAnyTopic();
            subscriberSocket.ReceiveReady += HandleReceivedEvent;

            poller = new NetMQPoller()
            {
                subscriberSocket
            };

            poller.RunAsync();
        }

        public T GetService<T>() where T : class
        {
            var interceptor = new InvocationInterceptor(typeof(T), requestConnectionString);

            var target = DummyTargetActivator.CreateInstance<T>();

            var proxy = (T)ProxyGenerator.CreateInterfaceProxyWithTarget(typeof(T), target, interceptor);
            
            targetInstances.Add(typeof(T).FullName, target);

            return proxy;
        }

        private void HandleReceivedEvent(object sender, NetMQSocketEventArgs e)
        {
            var eventJson = e.Socket.ReceiveFrameString();

            var eventData = JsonConvert.DeserializeObject<EventMessage>(eventJson, MessageSerializationSettings.Instance);

            var target = targetInstances[eventData.TypeName];

            InvokeEventOnTarget(target, eventData.EventName, eventData.EventArgs);
        }

        private static void InvokeEventOnTarget(object source, string eventName, object eventArgs)
        {
            ((Delegate)source
                .GetType()
                .GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(source))
                .DynamicInvoke(source, eventArgs);
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

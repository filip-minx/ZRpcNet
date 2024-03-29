﻿using Castle.DynamicProxy;
using Minx.ZRpcNet.Serialization;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Minx.ZRpcNet
{
    public class ZRpcClient : IZRpcClient, IDisposable
    {
        private readonly Dictionary<string, object> targetInstances = new Dictionary<string, object>();
        private static ProxyGenerator ProxyGenerator = new ProxyGenerator();

        private string requestConnectionString;
        private string eventConnectionString;

        internal SubscriberSocket subscriberSocket;
        private NetMQPoller poller;

        public ZRpcClientOptions Options { get; } = ZRpcClientOptions.Default;

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
            var interceptor = new InvocationInterceptor(typeof(T), requestConnectionString, Options);

            var target = DummyTargetActivator.CreateInstance<T>();

            var proxy = (T)ProxyGenerator.CreateInterfaceProxyWithTarget(typeof(T), target, interceptor);

            targetInstances.Add(typeof(T).FullName, target);

            return proxy;
        }

        private void HandleReceivedEvent(object sender, NetMQSocketEventArgs e)
        {
            var eventJson = e.Socket.ReceiveFrameString();

            var eventData = MessageSerializer.DeserializeMessage<EventMessage>(eventJson);

            var target = targetInstances[eventData.Type.TypeName];

            InvokeEventOnTarget(target, eventData.EventName, eventData.EventArgs);
        }

        private static void InvokeEventOnTarget(object source, string eventName, object eventArgs)
        {
            ((Delegate)source
                .GetType()
                .GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(source))
                ?.DynamicInvoke(source, eventArgs);
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

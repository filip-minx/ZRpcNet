using Castle.DynamicProxy;
using NetMQ.Sockets;
using System;

namespace Minx.zRPC.NET
{
    public class ZRpcClient : IDisposable
    {
        private static ProxyGenerator ProxyGenerator = new ProxyGenerator();

        private RequestSocket socket;

        public ZRpcClient(string address, int port)
        {
            socket = new RequestSocket($">tcp://{address}:{port}");
        }

        public T GetService<T>() where T : class
        {
            var interceptor = new InvocationInterceptor(typeof(T), socket);

            return (T)ProxyGenerator.CreateInterfaceProxyWithoutTarget(
                typeof(T),
                interceptor);
        }

        public void Dispose()
        {
            socket?.Dispose();
            socket = null;
        }
    }
}

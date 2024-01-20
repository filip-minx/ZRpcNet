using System;

namespace Minx.ZRpcNet.Connectivity
{
    public static class ConnectivityExtensions
    {
        public static ZRpcServer UseConnectivityService(this ZRpcServer server, Action<ConnectivityService> serviceHandler)
        {
            var service = new ConnectivityService();
            serviceHandler(service);
            server.RegisterService<IConnectivityService, ConnectivityService>(service);

            return server;
        }

        public static ZRpcClient UseConnectivityService(this ZRpcClient client, Action<ConnectivityStateMonitor> monitorCallback)
        {
            client.Options.Timeout = TimeSpan.FromSeconds(5);

            monitorCallback?.Invoke(new ConnectivityStateMonitor(client, TimeSpan.FromSeconds(5)));

            return client;
        }
    }
}

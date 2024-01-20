using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ZRpcDynamicAssembly")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Minx.ZRpcNet.Connectivity
{
    public interface IConnectivityService
    {
        event EventHandler<ConnectionChangedEventArgs> ClientConnectionChanged;

        void Heartbeat(string clientGuid);
    }
}

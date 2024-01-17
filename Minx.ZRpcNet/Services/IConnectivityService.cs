using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ZRpcDynamicAssembly")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Minx.ZRpcNet.Services
{
    public interface IConnectivityService
    {
        event EventHandler<ConnectionChangedEventArgs> ClientConnectionChanged;

        void Heartbeat(string clientGuid);
    }
}

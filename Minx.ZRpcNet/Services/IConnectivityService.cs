using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ZRpcDynamicAssembly")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Minx.ZRpcNet.Services
{
    internal interface IConnectivityService
    {
        void Heartbeat();
    }
}

using NetMQ;
using System;
using System.Collections.Concurrent;

namespace Minx.ZRpcNet
{
    public class ZRpcClientOptions
    {
        public static ZRpcClientOptions Default => new ZRpcClientOptions()
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };

        public TimeSpan Timeout { get; set; }

        public byte[] CurveServerPublicKey { get; set; }

        /// <summary>
        /// All services are enabled by default. You can disable specific services by adding them to this dictionary with a value of true.
        /// Services not present in this dictionary are considered enabled.
        /// </summary>
        public ConcurrentDictionary<string, bool> DisabledServices { get; } = new ConcurrentDictionary<string, bool>();
    }
}

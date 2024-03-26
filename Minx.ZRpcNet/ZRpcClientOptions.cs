using NetMQ;
using System;

namespace Minx.ZRpcNet
{
    public class ZRpcClientOptions
    {
        public static ZRpcClientOptions Default = new ZRpcClientOptions()
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };

        public TimeSpan Timeout { get; set; }

        public byte[] CurveServerPublicKey { get; set; }
    }
}

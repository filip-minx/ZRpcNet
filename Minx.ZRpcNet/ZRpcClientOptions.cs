using System;
using System.Collections.Generic;

namespace Minx.ZRpcNet
{
    public class ZRpcClientOptions
    {
        internal Dictionary<Type, TimeSpan> ServiceTimeouts = new Dictionary<Type, TimeSpan>();

        public static ZRpcClientOptions Default = new ZRpcClientOptions()
        {
            ServiceTimeouts = new Dictionary<Type, TimeSpan>()
        };

        internal TimeSpan GetTimeout(Type serviceType)
        {
            if (ServiceTimeouts.TryGetValue(serviceType, out var timeout))
            {
                return timeout;
            }
            else
            {
                return System.Threading.Timeout.InfiniteTimeSpan;
            }
        }

        public void SetTimeout(Type serviceType, TimeSpan timeout)
        {
            ServiceTimeouts[serviceType] = timeout;
        }
    }
}

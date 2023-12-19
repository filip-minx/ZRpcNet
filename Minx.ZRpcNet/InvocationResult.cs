using System;

namespace Minx.ZRpcNet
{
    public class InvocationResult
    {
        public object Result { get; set; }

        public Exception Exception { get; set; }
    }
}

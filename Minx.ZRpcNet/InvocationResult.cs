using System;

namespace Minx.ZRpcNet
{
    public class InvocationResult
    {
        public InvocationMessage Invocation { get; set; }

        public object Result { get; set; }

        public Exception Exception { get; set; }
    }
}

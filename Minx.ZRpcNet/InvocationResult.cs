using Minx.ZRpcNet.Serialization;
using System;

namespace Minx.ZRpcNet
{
    public class InvocationResult
    {
        public object Result { get; set; }
        public TypeLocator ResultType { get; set; }
        public Exception Exception { get; set; }
    }
}

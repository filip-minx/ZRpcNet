using System;

namespace Minx.ZRpcNet
{
    public class ZRpcInvocationException : Exception
    {
        public ZRpcInvocationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

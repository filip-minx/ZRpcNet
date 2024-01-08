using System;

namespace Minx.ZRpcNet
{
    public class ZRpcServiceNotRegisteredException : Exception
    {
        public ZRpcServiceNotRegisteredException(string message) : base(message)
        {
        }
    }
}

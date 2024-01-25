using Minx.ZRpcNet.Serialization;

namespace Minx.ZRpcNet
{
    public class InvocationMessage
    {
        public TypeLocator Type { get; set; }

        public string MethodName { get; set; }

        public object[] Arguments { get; set; }

        public TypeLocator[] ArgumentsTypes { get; set; }
    }
}

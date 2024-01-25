using Minx.ZRpcNet.Serialization;

namespace Minx.ZRpcNet
{
    class EventMessage
    {
        public TypeLocator Type { get; set; }

        public string EventName { get; set; }

        public object EventArgs { get; set; }
    }
}

namespace Minx.ZRpcNet
{
    class EventMessage
    {
        public string TypeName { get; set; }

        public string EventName { get; set; }

        public object EventArgs { get; set; }
    }
}

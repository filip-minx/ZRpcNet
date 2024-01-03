namespace Minx.ZRpcNet
{
    public class InvocationMessage
    {
        public string TypeName { get; set; }

        public string MethodName { get; set; }

        public object[] Arguments { get; set; }

        public string[] ArgumentsTypeNames { get; set; }
    }
}

namespace Minx.ZRpcNet
{
    public interface IZRpcServer
    {
        void RegisterService<TInterface, TImplementation>(TImplementation implementation)
            where TImplementation : class, TInterface;
    }
}

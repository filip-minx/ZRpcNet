namespace Minx.ZRpcNet
{
    public interface IZRpcServer
    {
        bool IsServiceRegistered<TInterface>();
        void RegisterService<TInterface, TImplementation>(TImplementation implementation)
            where TImplementation : class, TInterface;
        void UnregisterService<TInterface>();
    }
}

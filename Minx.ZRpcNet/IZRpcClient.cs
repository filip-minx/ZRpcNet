namespace Minx.ZRpcNet
{
    public interface IZRpcClient
    {
        T GetService<T>() where T : class;
    }
}

namespace Minx.ZRpcNet
{
    internal interface IZRpcClient
    {
        T GetService<T>() where T : class;
    }
}

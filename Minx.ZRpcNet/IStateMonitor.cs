using System;

namespace Minx.ZRpcNet
{
    public interface IStateMonitor<T>
    {
        T State { get; }

        event EventHandler<T> StateChanged;
    }
}
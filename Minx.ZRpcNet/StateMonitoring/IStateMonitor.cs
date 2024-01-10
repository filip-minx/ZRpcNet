using System;

namespace Minx.ZRpcNet.StateMonitoring
{
    public interface IStateMonitor<T>
    {
        T State { get; }

        event EventHandler<T> StateChanged;
    }
}
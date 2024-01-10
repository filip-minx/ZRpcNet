using Minx.ZRpcNet.Services;
using System;
using System.Timers;

namespace Minx.ZRpcNet.StateMonitoring
{
    public class ConnectivityStateMonitor : IStateMonitor<ConnectivityState>, IDisposable
    {
        private Timer heartbeatTimer;
        private readonly TimeSpan heartbeatTimeoutThreshold;
        private DateTime lastSuccessfulHeartbeat;

        private IConnectivityService connectivityService;

        public ConnectivityState State { get; private set; }

        public event EventHandler<ConnectivityState> StateChanged;

        public ConnectivityStateMonitor(ZRpcClient zRpcClient, TimeSpan timeoutThreshold)
        {
            State = ConnectivityState.Disconnected;

            connectivityService = zRpcClient.GetService<IConnectivityService>();

            heartbeatTimeoutThreshold = timeoutThreshold;
            heartbeatTimer = new Timer(1000);
            heartbeatTimer.Elapsed += (sender, e) => Tick();
            heartbeatTimer.Start();

            lastSuccessfulHeartbeat = DateTime.MinValue;
        }

        private void Tick()
        {
            try
            {
                connectivityService.Heartbeat();

                lastSuccessfulHeartbeat = DateTime.Now;

                if (State == ConnectivityState.Disconnected)
                {
                    State = ConnectivityState.Connected;
                    StateChanged?.Invoke(this, State);
                }
            }
            catch (TimeoutException)
            {
                if (DateTime.Now - lastSuccessfulHeartbeat > heartbeatTimeoutThreshold)
                {
                    if (State == ConnectivityState.Connected)
                    {
                        State = ConnectivityState.Disconnected;
                        StateChanged?.Invoke(this, State);
                    }
                }
            }
        }

        public void Dispose()
        {
            heartbeatTimer?.Dispose();
            heartbeatTimer = null;
        }
    }
}

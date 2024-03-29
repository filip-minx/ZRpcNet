﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Minx.ZRpcNet.Connectivity
{
    public class ConnectivityService : IConnectivityService, IDisposable
    {
        private Timer heartbeatTimer;
        private Dictionary<string, DateTime> lastHeartbeats = new Dictionary<string, DateTime>();
        public TimeSpan TimeoutThreshold { get; set; } = TimeSpan.FromSeconds(5);

        public event EventHandler<ConnectionChangedEventArgs> ClientConnectionChanged;

        public ConnectivityService()
        {
            heartbeatTimer = new Timer(1000);
            heartbeatTimer.Elapsed += CheckHeartbeats;
            heartbeatTimer.Start();
        }

        public void Heartbeat(string clientId)
        {
            lock (lastHeartbeats)
            {
                if (!lastHeartbeats.ContainsKey(clientId))
                {
                    ClientConnectionChanged?.Invoke(this,
                        new ConnectionChangedEventArgs(clientId, ConnectivityState.Connected));
                }

                lastHeartbeats[clientId] = DateTime.Now;
            }
        }

        private void CheckHeartbeats(object sender, ElapsedEventArgs e)
        {
            lock (lastHeartbeats)
            {
                var now = DateTime.Now;

                lastHeartbeats
                    .Where(id => now - id.Value > TimeoutThreshold)
                    .Select(id => id.Key)
                    .ToList()
                    .ForEach(id =>
                    {
                        lastHeartbeats.Remove(id);

                        ClientConnectionChanged?.Invoke(this,
                            new ConnectionChangedEventArgs(id, ConnectivityState.Disconnected));
                    });
            }
        }

        public void Dispose()
        {
            heartbeatTimer?.Dispose();
            heartbeatTimer = null;
        }
    }
}

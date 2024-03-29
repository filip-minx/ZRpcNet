﻿using System;

namespace Minx.ZRpcNet.Connectivity
{
    public class ConnectionChangedEventArgs : EventArgs
    {
        public string ClientId { get; }
        public ConnectivityState ConnectivityState { get; }

        public ConnectionChangedEventArgs(string clientId, ConnectivityState connectivityState)
        {
            ClientId = clientId;
            ConnectivityState = connectivityState;
        }
    }
}

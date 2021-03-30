using System;

namespace UniWorkforce.Events
{
    public class ConnectedStateChangedEventArgs : EventArgs
    {
        public ConnectedState ConnectedState { get; }

        public ConnectedStateChangedEventArgs(ConnectedState connectedState)
        {
            ConnectedState = connectedState;
        }
    }
}

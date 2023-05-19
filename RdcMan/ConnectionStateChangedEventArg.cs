using System;

namespace RdcMan
{
	public class ConnectionStateChangedEventArgs : EventArgs
	{
		public Server Server { get; private set; }

		public RdpClient.ConnectionState State { get; private set; }

		public ConnectionStateChangedEventArgs(Server server, RdpClient.ConnectionState state)
		{
			Server = server;
			State = state;
		}
	}
}

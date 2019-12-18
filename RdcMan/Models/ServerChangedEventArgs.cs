using System;

namespace RdcMan
{
	public class ServerChangedEventArgs : EventArgs
	{
		public ServerBase Server
		{
			get;
			private set;
		}

		public ChangeType ChangeType
		{
			get;
			private set;
		}

		public ServerChangedEventArgs(ServerBase server, ChangeType changeType)
		{
			Server = server;
			ChangeType = changeType;
		}
	}
}

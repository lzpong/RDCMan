using System;
using System.ComponentModel.Composition;

namespace RdcMan
{
	[Export(typeof(IBuiltInVirtualGroup))]
	internal class ConnectedGroup : BuiltInVirtualGroup<ConnectedServerRef>, IServerRefFactory
	{
		public static ConnectedGroup Instance { get; private set; }

		protected override string XmlNodeName => "connected";

		static ConnectedGroup()
		{
			Server.ConnectionStateChanged += Server_ConnectionStateChanged;
			Server.FocusReceived += Server_FocusReceived;
		}

		private ConnectedGroup()
		{
			base.Text = "ÒÑÁ¬½Ó";
			Instance = this;
		}

		private static void Server_FocusReceived(Server server)
		{
			ConnectedServerRef connectedServerRef = server.FindServerRef<ConnectedServerRef>();
			if (connectedServerRef != null)
			{
				connectedServerRef.LastFocusTime = DateTime.Now;
				if (ServerTree.Instance.SortGroup(Instance))
				{
					ServerTree.Instance.OnGroupChanged(Instance, ChangeType.InvalidateUI);
				}
			}
		}

		private static void Server_ConnectionStateChanged(ConnectionStateChangedEventArgs args)
		{
			switch (args.State)
			{
			case RdpClient.ConnectionState.Connected:
				Instance.AddReference(args.Server);
				break;
			case RdpClient.ConnectionState.Disconnected:
			{
				RdcTreeNode rdcTreeNode = args.Server.FindServerRef<ConnectedServerRef>();
				if (rdcTreeNode != null)
				{
					ServerTree.Instance.RemoveNode(rdcTreeNode);
				}
				break;
			}
			}
		}

		protected override bool ShouldWriteNode(RdcTreeNode node, FileGroup file) => file == null;

		public override bool CanRemoveChildren() => false;

		public override void Disconnect()
		{
			Hide();
			base.Disconnect();
		}

		public ServerRef Create(Server server) => new ConnectedServerRef(server);
	}
}

using System.Threading;
using System.Windows.Forms;

namespace RdcMan
{
	internal class ReconnectServerRef : ServerRef
	{
		private bool _selectedInConnectedGroup;

		public bool NeedToReconnect
		{
			get;
			private set;
		}

		private bool RemoveAfterConnection
		{
			get;
			set;
		}

		static ReconnectServerRef()
		{
			Server.ConnectionStateChanged += Server_ConnectionStateChanged;
		}

		private static void Server_ConnectionStateChanged(ConnectionStateChangedEventArgs args)
		{
			ReconnectServerRef reconnectServerRef = args.Server.FindServerRef<ReconnectServerRef>();
			if (reconnectServerRef != null)
			{
				switch (args.State)
				{
				case RdpClient.ConnectionState.Connecting:
					reconnectServerRef.OnConnecting();
					break;
				case RdpClient.ConnectionState.Connected:
					reconnectServerRef.OnConnected();
					break;
				case RdpClient.ConnectionState.Disconnected:
					reconnectServerRef.OnDisconnected();
					break;
				}
			}
		}

		public ReconnectServerRef(Server server)
			: base(server)
		{
		}

		public void Start(bool removeAfterConnection)
		{
			RemoveAfterConnection = removeAfterConnection;
			NeedToReconnect = true;
			ConnectedServerRef connectedServerRef = ServerNode.FindServerRef<ConnectedServerRef>();
			if (connectedServerRef != null)
			{
				_selectedInConnectedGroup = connectedServerRef.IsSelected;
				if (_selectedInConnectedGroup)
				{
					ServerTree.Instance.SelectedNode = ServerNode;
				}
			}
			if (!ServerNode.IsConnected)
			{
				ServerNode.Connect();
			}
			else
			{
				ServerNode.Disconnect();
			}
		}

		public override bool CanRemove(bool popUI)
		{
			return true;
		}

		public override void Reconnect()
		{
			NeedToReconnect = true;
			ServerNode.Disconnect();
		}

		public override void Disconnect()
		{
			NeedToReconnect = false;
			base.Disconnect();
		}

		public override void LogOff()
		{
			NeedToReconnect = false;
			base.LogOff();
		}

		private void OnConnecting()
		{
			if (RemoveAfterConnection)
			{
				NeedToReconnect = false;
			}
		}

		private void OnConnected()
		{
			NeedToReconnect = false;
			if (RemoveAfterConnection)
			{
				if (ServerTree.Instance.SelectedNode == this)
				{
					ServerTree.Instance.SelectedNode = ServerNode;
				}
				else if (_selectedInConnectedGroup && ServerTree.Instance.SelectedNode == ServerNode)
				{
					ServerTree.Instance.SelectedNode = ServerNode.FindServerRef<ConnectedServerRef>();
				}
				ServerTree.Instance.RemoveNode(this);
			}
		}

		private void OnDisconnected()
		{
			if (NeedToReconnect)
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					ServerNode.ParentForm.Invoke((MethodInvoker)delegate
					{
						ServerNode.Connect();
					});
				});
			}
			else
			{
				ServerTree.Instance.RemoveNode(this);
			}
		}
	}
}

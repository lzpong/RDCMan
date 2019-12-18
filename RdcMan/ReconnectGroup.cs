using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace RdcMan
{
	[Export(typeof(IBuiltInVirtualGroup))]
	internal class ReconnectGroup : BuiltInVirtualGroup<ReconnectServerRef>, IServerRefFactory
	{
		public static ReconnectGroup Instance { get; private set; }

		public override string ConfigName => "RecentlyUsed";
		private ReconnectGroup()
		{
			base.Text = "÷ÿ¡¨Ω”";
			Instance = this;
		}

		public override bool CanDropServers()
		{
			return true;
		}

		public override DragDropEffects DropBehavior()
		{
			return DragDropEffects.Copy;
		}

		public override bool HandleMove(RdcTreeNode childNode)
		{
			ReconnectServerRef reconnectServerRef = AddReference(childNode as ServerBase);
			reconnectServerRef.Start(removeAfterConnection: false);
			return true;
		}

		public ServerRef Create(Server server)
		{
			return new ReconnectServerRef(server);
		}
	}
}

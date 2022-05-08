using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace RdcMan {
	[Export(typeof(IBuiltInVirtualGroup))]
	internal class ReconnectGroup : BuiltInVirtualGroup<ReconnectServerRef>, IServerRefFactory {
		public static ReconnectGroup Instance { get; private set; }

		protected override string XmlNodeName => "reconnect";

		private ReconnectGroup() {
			base.Text = "重新连接";
			Instance = this;
		}

		public override bool CanDropServers() {
			return true;
		}

		public override DragDropEffects DropBehavior() {
			return DragDropEffects.Copy;
		}

		public override bool HandleMove(RdcTreeNode childNode) {
			ReconnectServerRef reconnectServerRef = AddReference(childNode as ServerBase);
			reconnectServerRef.Start(removeAfterConnection: false);
			return true;
		}

		public ServerRef Create(Server server) {
			return new ReconnectServerRef(server);
		}
	}
}

using System.ComponentModel.Composition;
using RdcMan;

namespace RdcMan.Ctrl {
	/// <summary>
	/// 内部组,需登录获取组配置
	/// </summary>
	[Export(typeof(IBuiltInVirtualGroup))]
	internal class InternalGroup : BuiltInVirtualGroup<InternalServerRef>, IServerRefFactory {
		public static InternalGroup Instance { get; private set; }

		protected override string XmlNodeName => "InternalGroups";
		public override string ConfigName => "Internal";

		public bool isLogin { get; private set; }
		private InternalGroup() {
			base.Text = "内部组(未登录)";
			Instance = this;
			isLogin = false;
		}

		public override InternalServerRef AddReference(ServerBase serverBase) {
			base.IsInTree = true;
			return base.AddReference(serverBase);
		}

		public override bool CanDropServers() {
			return false;
		}

		public override bool HandleMove(RdcTreeNode childNode) {
			return false;
			//AddReference(childNode as ServerBase);
			//return true;
		}

		public ServerRef Create(Server server) {
			return new InternalServerRef(server);
		}

		public bool Login(string user, string pswd) {


			base.Text = "内部组("+user+")";
			return false;
		}

		public void Logout() {
			base.Text = "内部组(未登录)";
		}
	}
}

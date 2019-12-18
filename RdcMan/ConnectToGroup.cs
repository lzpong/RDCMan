using System;
using System.ComponentModel.Composition;

namespace RdcMan
{
	[Export(typeof(IBuiltInVirtualGroup))]
	internal class ConnectToGroup : BuiltInVirtualGroup<ServerRef>, IServerRefFactory
	{
		public static ConnectToGroup Instance
		{
			get;
			private set;
		}

		protected override bool IsVisibilityConfigurable => false;

		private ConnectToGroup()
		{
			base.Text = "Á¬½Óµ½";
			Instance = this;
		}

		public override ServerRef AddReference(ServerBase serverBase)
		{
			throw new InvalidOperationException();
		}

		public override void InvalidateNode()
		{
			base.InvalidateNode();
			if (base.Nodes.Count == 0)
			{
				base.IsInTree = false;
			}
		}

		public ServerRef Create(Server server)
		{
			throw new NotImplementedException("ConnectTo does not contain ServerRef");
		}
	}
}

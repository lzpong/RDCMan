using System.ComponentModel.Composition;

namespace RdcMan
{
	[Export(typeof(IBuiltInVirtualGroup))]
	internal class FavoritesGroup : BuiltInVirtualGroup<FavoriteServerRef>, IServerRefFactory
	{
		public static FavoritesGroup Instance { get; private set; }

		protected override string XmlNodeName => "favorites";
		public override string ConfigName => "Favorites";

		private FavoritesGroup()
		{
			base.Text = " ’≤ÿ";
			Instance = this;
		}

		public override FavoriteServerRef AddReference(ServerBase serverBase)
		{
			base.IsInTree = true;
			return base.AddReference(serverBase);
		}

		public override bool CanDropServers()
		{
			return true;
		}

		public override bool HandleMove(RdcTreeNode childNode)
		{
			AddReference(childNode as ServerBase);
			return true;
		}

		public ServerRef Create(Server server)
		{
			return new FavoriteServerRef(server);
		}
	}
}

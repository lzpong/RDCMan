using System.Windows.Forms;

namespace RdcMan
{
	internal class RemoteDesktopsMenuItem : RdcMenuItem
	{
		//private const string AllItem = "[All]";

		private bool HasChangedSinceMenuUpdate { get; set; }

		public RemoteDesktopsMenuItem()
			: base("Ô¶³Ì×ÀÃæ")
		{
			base.Name = MenuNames.RemoteDesktops.ToString();
			ServerTree.Instance.GroupChanged += GroupChanged;
			ServerTree.Instance.ServerChanged += ServerChanged;
			HasChangedSinceMenuUpdate = true;
		}

		private void ServerChanged(ServerChangedEventArgs e)
		{
			if (e.ChangeType.HasFlag(ChangeType.TreeChanged) || e.ChangeType.HasFlag(ChangeType.PropertyChanged))
			{
				HasChangedSinceMenuUpdate = true;
			}
		}

		private void GroupChanged(GroupChangedEventArgs e)
		{
			if (e.ChangeType.HasFlag(ChangeType.TreeChanged) || e.ChangeType.HasFlag(ChangeType.PropertyChanged))
			{
				HasChangedSinceMenuUpdate = true;
			}
		}

		public override void Update()
		{
			if (!HasChangedSinceMenuUpdate)
			{
				return;
			}
			HasChangedSinceMenuUpdate = false;
			base.DropDownItems.Clear();
			foreach (TreeNode node in ServerTree.Instance.Nodes)
			{
				PopulateRemoteDesktopsMenuItems(base.DropDownItems, node);
			}
		}

		protected override void OnClick()
		{
		}

		private void PopulateRemoteDesktopsMenuItems(ToolStripItemCollection items, TreeNode treeNode)
		{
			RdcTreeNode rdcTreeNode = treeNode as RdcTreeNode;
			ToolStripMenuItem toolStripMenuItem;
			if (rdcTreeNode is GroupBase groupBase && (groupBase.Nodes.Count > 1 || groupBase.HasGroups))
			{
				toolStripMenuItem = new ToolStripMenuItem(rdcTreeNode.Text);
				ServerMenuItem serverMenuItem = new ServerMenuItem(rdcTreeNode) {
					Text = "[All]"
				};
				toolStripMenuItem.DropDownItems.Add(serverMenuItem);
			}
			else
			{
				toolStripMenuItem = new ServerMenuItem(rdcTreeNode);
			}
			foreach (TreeNode node in rdcTreeNode.Nodes)
			{
				PopulateRemoteDesktopsMenuItems(toolStripMenuItem.DropDownItems, node);
			}
			items.Add(toolStripMenuItem);
		}
	}
}

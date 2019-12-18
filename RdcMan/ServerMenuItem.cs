namespace RdcMan
{
	internal class ServerMenuItem : RdcMenuItem
	{
		public ServerMenuItem(RdcTreeNode node)
		{
			base.Tag = node;
			Text = node.Text;
		}

		public override void Update()
		{
			if (base.DropDownItems.Count == 0)
			{
				base.Checked = (ServerTree.Instance.SelectedNode == base.Tag);
			}
		}

		protected override void OnClick()
		{
			ServerTree.Instance.SelectedNode = (RdcTreeNode)base.Tag;
		}
	}
}

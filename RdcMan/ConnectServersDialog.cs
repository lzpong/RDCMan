using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan
{
	internal class ConnectServersDialog : SelectServersDialogBase
	{
		public ConnectServersDialog(IEnumerable<ServerBase> servers)
			: base("连接服务器", "连接")
		{
			int rowIndex = 0;
			int tabIndex = 0;
			AddLabel("选择要连接的服务器", ref rowIndex, ref tabIndex);
			AddListView(ref rowIndex, ref tabIndex);
			InitButtons();
			this.ScaleAndLayout();
			Action<ServerBase> action = delegate(ServerBase server)
			{
				base.ListView.Items.Add(CreateListViewItem(server));
			};
			servers.ForEach(action);
			base.ListView.ItemChecked += ListView_ItemChecked;
			_acceptButton.Enabled = false;
		}

		private void ListView_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			_acceptButton.Enabled = (base.ListView.CheckedItems.Count > 0);
		}
	}
}

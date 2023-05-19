using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan
{
	internal class ConnectServersDialog : SelectServersDialogBase
	{
		public ConnectServersDialog(IEnumerable<ServerBase> servers)
			: base("连接服务器", "连接(&C)")
		{
			int rowIndex = 0;
			int num = 0;
			AddLabel("选择要连接的服务器", ref rowIndex, ref num);
			AddListView(ref rowIndex, ref num);
			InitButtons();
			this.ScaleAndLayout();
			servers.ForEach(delegate(ServerBase server)
			{
				base.ListView.Items.Add(CreateListViewItem(server));
			});
			base.ListView.ItemChecked += ListView_ItemChecked;
			_acceptButton.Enabled = false;
		}

		private void ListView_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			_acceptButton.Enabled = base.ListView.CheckedItems.Count > 0;
		}
	}
}

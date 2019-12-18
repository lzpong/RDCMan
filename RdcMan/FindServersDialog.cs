using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RdcMan
{
	internal class FindServersDialog : SelectServersDialogBase
	{
		private string _previousFilterText;

		private List<Server> _servers;

		private TextBox _filterTextBox;

		public FindServersDialog()
			: base("查找服务", "选择")
		{
			int rowIndex = 0;
			int tabIndex = 0;
			AddLabel("输入以过滤服务器", ref rowIndex, ref tabIndex);
			_filterTextBox = FormTools.NewTextBox(0, rowIndex++, tabIndex++);
			_filterTextBox.Enabled = true;
			_filterTextBox.Width = 500;
			_filterTextBox.TextChanged += Filter_TextChanged;
			base.Controls.Add(_filterTextBox);
			AddListView(ref rowIndex, ref tabIndex);
			base.ListView.ContextMenuStrip = new ContextMenuStrip();
			base.ListView.ContextMenuStrip.Opening += ContextMenuPopup;
			InitButtons();
			this.ScaleAndLayout();
			_previousFilterText = string.Empty;
			CollectServers();
			PopulateList();
		}

		protected override void OnClosed(EventArgs e)
		{
			if (base.ListView.CheckedItems.Count == 0 && base.ListView.Items.Count > 0)
			{
				if (base.ListView.FocusedItem == null)
				{
					base.ListView.FocusedItem = base.ListView.Items[0];
				}
				base.ListView.FocusedItem.Checked = true;
			}
		}

		private void CollectServers()
		{
			_servers = new List<Server>();
			ServerTree.Instance.Nodes.VisitNodes(delegate(RdcTreeNode node)
			{
				Server server = node as Server;
				if (server != null)
				{
					_servers.Add(server);
				}
			});
		}

		private void PopulateList()
		{
			try
			{
				Regex regex = new Regex(_filterTextBox.Text, RegexOptions.IgnoreCase | RegexOptions.Compiled);
				base.ListView.BeginUpdate();
				SuspendItemChecked();
				base.ListView.Items.Clear();
				foreach (Server server in _servers)
				{
					if (regex.IsMatch(server.FullPath))
					{
						base.ListView.Items.Add(CreateListViewItem(server));
					}
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				ResumeItemChecked();
				base.ListView.EndUpdate();
			}
		}

		private void FilterList()
		{
			try
			{
				Regex regex = new Regex(_filterTextBox.Text, RegexOptions.IgnoreCase | RegexOptions.Compiled);
				int num = 0;
				while (num < base.ListView.Items.Count)
				{
					Server server = base.ListView.Items[num].Tag as Server;
					if (!regex.IsMatch(server.FullPath))
					{
						base.ListView.Items.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
			}
			catch (Exception)
			{
			}
		}

		private void ContextMenuPopup(object menuSender, EventArgs args)
		{
			ContextMenuStrip contextMenuStrip = menuSender as ContextMenuStrip;
			List<ServerBase> selectedServers = base.SelectedServers.ToList();
			if (selectedServers.Count == 0)
			{
				if (base.ListView.FocusedItem == null)
				{
					return;
				}
				selectedServers.Add(base.ListView.FocusedItem.Tag as ServerBase);
			}
			contextMenuStrip.Items.Clear();
			NodeHelper.AnyOrAllConnected(selectedServers, out bool anyConnected, out bool allConnected);
			ToolStripMenuItem toolStripMenuItem = new DelegateMenuItem("&C连接", MenuNames.SessionConnect, delegate
			{
				NodeHelper.ThrottledConnect(selectedServers);
				OK();
			});
			toolStripMenuItem.Enabled = !allConnected;
			contextMenuStrip.Items.Add(toolStripMenuItem);
			toolStripMenuItem = new DelegateMenuItem("&R重新连接", MenuNames.SessionReconnect, delegate
			{
				selectedServers.ForEach(delegate(ServerBase server)
				{
					server.Reconnect();
				});
				OK();
			});
			toolStripMenuItem.Enabled = anyConnected;
			contextMenuStrip.Items.Add(toolStripMenuItem);
			toolStripMenuItem = new DelegateMenuItem("&D断开连接", MenuNames.SessionDisconnect, delegate
			{
				NodeHelper.ThrottledDisconnect(selectedServers);
				OK();
			});
			toolStripMenuItem.Enabled = anyConnected;
			contextMenuStrip.Items.Add(toolStripMenuItem);
			contextMenuStrip.Items.Add("-");
			toolStripMenuItem = new DelegateMenuItem("注销", MenuNames.SessionLogOff, delegate
			{
				selectedServers.ForEach(delegate(ServerBase server)
				{
					server.LogOff();
				});
				OK();
			});
			toolStripMenuItem.Enabled = (!Policies.DisableLogOff && anyConnected);
			contextMenuStrip.Items.Add(toolStripMenuItem);
			contextMenuStrip.Items.Add("-");
			toolStripMenuItem = new DelegateMenuItem("删除", MenuNames.EditRemove, delegate
			{
				if (!anyConnected || FormTools.YesNoDialog("有活动的会话。 你确定吗？") == DialogResult.Yes)
				{
					selectedServers.ForEach(delegate(ServerBase server)
					{
						ServerTree.Instance.ConfirmRemove(server, askUser: false);
					});
					Cancel();
				}
			});
			contextMenuStrip.Items.Add(toolStripMenuItem);
			contextMenuStrip.Items.Add("-");
			contextMenuStrip.Items.Add(new DelegateMenuItem("添加到收藏夹", MenuNames.EditAddToFavorites, delegate
			{
				selectedServers.ForEach(delegate(ServerBase server)
				{
					FavoritesGroup.Instance.AddReference(server);
				});
				OK();
			}));
		}

		private void Filter_TextChanged(object sender, EventArgs e)
		{
			if (_filterTextBox.Text.StartsWith(_previousFilterText))
			{
				FilterList();
			}
			else
			{
				PopulateList();
			}
			_previousFilterText = _filterTextBox.Text;
		}
	}
}

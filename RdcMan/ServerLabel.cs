using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	internal class ServerLabel : Button
	{
		private static ContextMenuStrip _menu;

		public new static int Height
		{
			get;
			private set;
		}

		public ServerBase AssociatedNode
		{
			get;
			private set;
		}

		public Server Server
		{
			get;
			private set;
		}

		public int ThumbnailIndex
		{
			get;
			set;
		}

		static ServerLabel()
		{
			_menu = new ContextMenuStrip();
			_menu.Opening += MenuPopup;
			Button button = new Button
			{
				FlatStyle = FlatStyle.Flat,
				Font = new Font(ServerTree.Instance.Font, FontStyle.Bold)
			};
			Height = button.Height;
		}

		public ServerLabel(ServerBase node)
		{
			AssociatedNode = node;
			Server = node.ServerNode;
			base.Enabled = true;
			base.TabStop = true;
			ContextMenuStrip = _menu;
			TextAlign = ContentAlignment.MiddleCenter;
			base.FlatStyle = FlatStyle.Flat;
			Font = new Font(ServerTree.Instance.Font, FontStyle.Bold);
			Hide();
			CopyServerData();
			UpdateVisual();
		}

		public void CopyServerData()
		{
			Text = Server.DisplayName;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.Clicks == 1)
				{
					Focus();
					return;
				}
				ServerTree.Instance.SelectedNode = AssociatedNode;
				Server.Connect();
				Server.Focus();
			}
		}

		private static void MenuPopup(object sender, CancelEventArgs e)
		{
			_menu.Items.Clear();
			ServerLabel serverLabel = (sender as ContextMenuStrip).SourceControl as ServerLabel;
			ServerBase server = serverLabel.AssociatedNode;
			MenuHelper.AddSessionMenuItems(_menu, server);
			_menu.Items.Add("-");
			_menu.Items.Add(new DelegateMenuItem("E&xpand", MenuNames.SessionExpand, delegate
			{
				ServerTree.Instance.SelectedNode = server;
				if (server.IsConnected)
				{
					server.Focus();
				}
			}));
			MenuHelper.AddDockingMenuItems(_menu, server);
			_menu.Items.Add("-");
			MenuHelper.AddMaintenanceMenuItems(_menu, server);
			Program.PluginAction(delegate(IPlugin p)
			{
				p.OnContextMenu(_menu, server);
			});
			e.Cancel = false;
		}

		protected override void OnGotFocus(EventArgs e)
		{
			UpdateVisual();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			Program.TheForm.RecordLastFocusedServerLabel(this);
			UpdateVisual();
		}

		protected void UpdateVisual()
		{
			if (Focused)
			{
				ForeColor = SystemColors.ActiveCaptionText;
				BackColor = SystemColors.ActiveCaption;
			}
			else
			{
				ForeColor = SystemColors.InactiveCaptionText;
				BackColor = SystemColors.InactiveCaption;
			}
		}
	}
}

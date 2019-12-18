using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	internal class ServerForm : RdcBaseForm, IUndockedServerForm
	{
		private static readonly Dictionary<Keys, Action<ServerForm>> Shortcuts;

		private static readonly List<ServerForm> ServerForms;

		private ToolStripMenuItem _sessionConnectServerMenuItem;

		private ToolStripMenuItem _sessionConnectServerAsMenuItem;

		private ToolStripMenuItem _sessionReconnectServerMenuItem;

		private ToolStripMenuItem _sessionSendKeysMenuItem;

		private ToolStripMenuItem _sessionRemoteActionsMenuItem;

		private ToolStripMenuItem _sessionDisconnectServerMenuItem;

		private ToolStripMenuItem _sessionFullScreenMenuItem;

		private ToolStripMenuItem _sessionScreenCaptureMenuItem;

		private Size _clientSize;

		private readonly Server _server;

		MenuStrip IUndockedServerForm.MainMenuStrip => _menuStrip;

		ServerBase IUndockedServerForm.Server => _server;

		static ServerForm()
		{
			ServerForms = new List<ServerForm>();
			Shortcuts = new Dictionary<Keys, Action<ServerForm>>
			{
				{
					Keys.Return,
					delegate(ServerForm f)
					{
						f._server.Connect();
					}
				},
				{
					Keys.LButton | Keys.MButton | Keys.Back | Keys.Shift,
					delegate(ServerForm f)
					{
						f._server.DoConnectAs();
					}
				},
				{
					Keys.LButton | Keys.MButton | Keys.Back | Keys.Alt,
					delegate(ServerForm f)
					{
						f._server.DoPropertiesDialog();
					}
				}
			};
			ServerTree.Instance.GroupChanged += OnGroupChanged;
			ServerTree.Instance.ServerChanged += OnServerChanged;
		}

		private static void OnGroupChanged(GroupChangedEventArgs e)
		{
			if (e.ChangeType.HasFlag(ChangeType.PropertyChanged))
			{
				using (Helpers.Timer("updating server form settings from group {0}", e.Group.Text))
				{
					if (e.Group == ServerTree.Instance.RootNode)
					{
						UpdateFromGlobalSettings();
					}
					UpdateFromServerSettings();
				}
			}
		}

		private static void OnServerChanged(ServerChangedEventArgs e)
		{
			if (e.ChangeType.HasFlag(ChangeType.PropertyChanged))
			{
				using (Helpers.Timer("updating server form settings from server {0}", e.Server.DisplayName))
				{
					UpdateFromServerSettings();
				}
			}
		}

		private static void UpdateFromServerSettings()
		{
			ServerForms.ForEach(delegate(ServerForm f)
			{
				f._server.InheritSettings();
				f._server.SetClientSizeProperties();
				f.SetTitle();
			});
		}

		public ServerForm(Server server)
		{
			_server = server;
			server.InheritSettings();
			base.Icon = Program.TheForm.Icon;
			SetTitle();
			Size clientSize = (!server.RemoteDesktopSettings.DesktopSizeSameAsClientAreaSize.Value && !server.RemoteDesktopSettings.DesktopSizeFullScreen.Value) ? server.RemoteDesktopSettings.DesktopSize.Value : Program.TheForm.GetClientSize();
			CreateMainMenu();
			SetMainMenuVisibility();
			SetClientSize(clientSize);
			this.ScaleAndLayout();
			base.Controls.Add(_server.Client.Control);
			_server.SetClientSizeProperties();
			ServerForms.Add(this);
		}

		private static void UpdateFromGlobalSettings()
		{
			ServerForms.ForEach(delegate(ServerForm f)
			{
				f.SetMainMenuVisibility();
				f.SetClientSize(f._clientSize);
			});
		}

		public override void SetClientSize(Size size)
		{
			int num = (!Program.Preferences.HideMainMenu) ? _menuPanel.Height : 0;
			base.ClientSize = new Size(size.Width, size.Height + num);
		}

		public override Size GetClientSize()
		{
			return _clientSize;
		}

		protected override void OnShown(EventArgs e)
		{
			_server.Client.Control.Show();
		}

		protected override void OnClosed(EventArgs e)
		{
			ServerForms.Remove(this);
			_server.LeaveFullScreen();
			base.Controls.Remove(_server.Client.Control);
			_server.Dock();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			int num = (!Program.Preferences.HideMainMenu) ? _menuPanel.Height : 0;
			_clientSize = new Size(base.ClientSize.Width, base.ClientSize.Height - num);
			LayoutContent();
		}

		protected override void LayoutContent()
		{
			int y = (!Program.Preferences.HideMainMenu) ? _menuPanel.Height : 0;
			_server.Client.Control.Bounds = new Rectangle(0, y, _clientSize.Width, _clientSize.Height);
			_menuPanel.Width = base.ClientSize.Width;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (!_menuStrip.IsActive && Shortcuts.TryGetValue(keyData, out Action<ServerForm> value))
			{
				value(this);
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		protected void CreateMainMenu()
		{
			_sessionConnectServerMenuItem = new DelegateMenuItem("&Connect server", MenuNames.SessionConnect, "Enter", delegate
			{
				_server.Connect();
			});
			_sessionConnectServerAsMenuItem = new DelegateMenuItem("Connect server &as...", MenuNames.SessionConnectAs, "Shift+Enter", delegate
			{
				_server.DoConnectAs();
			});
			_sessionReconnectServerMenuItem = new DelegateMenuItem("R&econnect server", MenuNames.SessionReconnect, delegate
			{
				_server.Reconnect();
			});
			_sessionDisconnectServerMenuItem = new DelegateMenuItem("&Disconnect server", MenuNames.SessionDisconnect, delegate
			{
				_server.Disconnect();
			});
			_sessionFullScreenMenuItem = new DelegateMenuItem("&Full screen", MenuNames.SessionFullScreen, delegate
			{
				_server.Client.MsRdpClient.FullScreen = true;
			});
			DelegateMenuItem value = new DelegateMenuItem("Doc&k", MenuNames.SessionDock, delegate
			{
				Close();
			});
			_sessionScreenCaptureMenuItem = new DelegateMenuItem("Screen capture", MenuNames.SessionScreenCapture, delegate
			{
				_server.ScreenCapture();
			});
			DelegateMenuItem value2 = new DelegateMenuItem("P&roperties", MenuNames.EditProperties, "Alt+Enter", delegate
			{
				_server.DoPropertiesDialog();
			});
			ToolStripMenuItem toolStripMenuItem = _menuStrip.Add("&Session", MenuNames.Session);
			toolStripMenuItem.DropDownItems.Add(_sessionConnectServerMenuItem);
			toolStripMenuItem.DropDownItems.Add(_sessionConnectServerAsMenuItem);
			toolStripMenuItem.DropDownItems.Add(_sessionReconnectServerMenuItem);
			toolStripMenuItem.DropDownItems.Add("-");
			_sessionSendKeysMenuItem = toolStripMenuItem.DropDownItems.Add("Send keys", MenuNames.SessionSendKeys);
			MenuHelper.AddSendKeysMenuItems(_sessionSendKeysMenuItem, () => _server);
			if (RdpClient.SupportsRemoteSessionActions)
			{
				_sessionRemoteActionsMenuItem = toolStripMenuItem.DropDownItems.Add("Remote actions", MenuNames.SessionRemoteActions);
				MenuHelper.AddRemoteActionsMenuItems(_sessionRemoteActionsMenuItem, () => _server);
			}
			toolStripMenuItem.DropDownItems.Add("-");
			toolStripMenuItem.DropDownItems.Add(_sessionDisconnectServerMenuItem);
			toolStripMenuItem.DropDownItems.Add("-");
			toolStripMenuItem.DropDownItems.Add(_sessionFullScreenMenuItem);
			toolStripMenuItem.DropDownItems.Add(value);
			toolStripMenuItem.DropDownItems.Add("-");
			toolStripMenuItem.DropDownItems.Add(_sessionScreenCaptureMenuItem);
			toolStripMenuItem.DropDownItems.Add("-");
			toolStripMenuItem.DropDownItems.Add(value2);
			ToolStripMenuItem toolStripMenuItem2 = _menuStrip.Add("&View", MenuNames.View);
			ToolStripMenuItem toolStripMenuItem3 = toolStripMenuItem2.DropDownItems.Add("&Client size", MenuNames.ViewClientSize);
			Size[] stockSizes = SizeHelper.StockSizes;
			foreach (Size size in stockSizes)
			{
				ClientSizeCheckedMenuItem value3 = new ClientSizeCheckedMenuItem(this, size);
				toolStripMenuItem3.DropDownItems.Add(value3);
			}
			toolStripMenuItem3.DropDownItems.Add(new CustomClientSizeCheckedMenuItem(this, "&Custom"));
			toolStripMenuItem3.DropDownItems.Add(new ToolStripMenuItem("From remote desktop size", null, delegate
			{
				SetClientSize(_server.IsConnected ? _server.Client.DesktopSize : _server.RemoteDesktopSettings.DesktopSize.Value);
			}));
		}

		protected override void UpdateMainMenu()
		{
			UpdateMenuItems(_menuStrip.Items);
			bool isConnected = _server.IsConnected;
			_sessionConnectServerMenuItem.Enabled = !isConnected;
			_sessionConnectServerAsMenuItem.Enabled = !isConnected;
			_sessionReconnectServerMenuItem.Enabled = isConnected;
			_sessionSendKeysMenuItem.Enabled = isConnected;
			if (RdpClient.SupportsRemoteSessionActions)
			{
				_sessionRemoteActionsMenuItem.Enabled = isConnected;
			}
			_sessionDisconnectServerMenuItem.Enabled = isConnected;
			_sessionFullScreenMenuItem.Enabled = isConnected;
			_sessionScreenCaptureMenuItem.Enabled = isConnected;
		}

		private void SetTitle()
		{
			Text = _server.GetQualifiedNameForUI();
		}
	}
}

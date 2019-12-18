using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Win32;

namespace RdcMan
{
	internal class MainForm : RdcBaseForm, IMainForm
	{
		private const int MinimumRemoteDesktopPanelHeight = 200;

		private const int AutoHideIntervalInMilliseconds = 250;

		private readonly Dictionary<Keys, Action> Shortcuts;

		private readonly Dictionary<Keys, Action<RdcTreeNode>> SelectedNodeShortcuts;

		private ToolStripMenuItem _fileSaveMenuItem;

		private ToolStripMenuItem _fileSaveAsMenuItem;

		private ToolStripMenuItem _fileCloseMenuItem;

		private ToolStripMenuItem _editPropertiesMenuItem;

		private ToolStripMenuItem _sessionConnectMenuItem;

		private ToolStripMenuItem _sessionConnectAsMenuItem;

		private ToolStripMenuItem _sessionReconnectMenuItem;

		private ToolStripMenuItem _sessionSendKeysMenuItem;

		private ToolStripMenuItem _sessionRemoteActionsMenuItem;

		private ToolStripMenuItem _sessionDisconnectMenuItem;

		private ToolStripMenuItem _sessionLogOffMenuItem;

		private ToolStripMenuItem _sessionListSessionsMenuItem;

		private ToolStripMenuItem _sessionFullScreenMenuItem;

		private ToolStripMenuItem _sessionUndockMenuItem;

		private ToolStripMenuItem _sessionUndockAndConnectMenuItem;

		private ToolStripMenuItem _sessionDockMenuItem;

		private ToolStripMenuItem _sessionScreenCaptureMenuItem;

		private Splitter _treeSplitter;

		private bool _allowSizeChanged;

		private Panel _remoteDesktopPanel;

		private Timer _autoSaveTimer;

		private ClientPanel _clientPanel;

		private Server _fullScreenServer;

		private Control[] _savedControls;

		private bool _areShuttingDown;

		private Timer _serverTreeAutoHideTimer;

		public bool IsFullScreen => _fullScreenServer != null;

		public string DescriptionText { get; private set; }

		public string VersionText { get; private set; }

		public string BuildText { get; private set; }

		public bool IsInternalVersion { get; private set; }

		public DockStyle ServerTreeLocation
		{
			get
			{
				return Program.Preferences.ServerTreeLocation;
			}
			set
			{
				Program.Preferences.ServerTreeLocation = value;
				_treeSplitter.Dock = value;
				ServerTree.Instance.Dock = value;
			}
		}

		public ControlVisibility ServerTreeVisibility
		{
			get
			{
				return Program.Preferences.ServerTreeVisibility;
			}
			set
			{
				Program.Preferences.ServerTreeVisibility = value;
				Size clientSize = GetClientSize();
				UpdateServerTreeVisibility(value);
				SetClientSize(clientSize);
			}
		}

		MenuStrip IMainForm.MainMenuStrip => _menuStrip;

		private MainForm() {
			Dictionary<Keys, Action> dictionary = new Dictionary<Keys, Action>();
			dictionary.Add((Keys)131150, OnFileNew);
			dictionary.Add((Keys)131151, OnFileOpen);
			dictionary.Add((Keys)131155, OnFileSave);
			dictionary.Add((Keys)131137, AddNodeDialogHelper.AddServersDialog);
			dictionary.Add((Keys)131143, AddNodeDialogHelper.AddGroupDialog);
			dictionary.Add((Keys)131142, MenuHelper.FindServers);
			dictionary.Add((Keys)131153, MenuHelper.ConnectTo);
			Shortcuts = dictionary;
			SelectedNodeShortcuts = new Dictionary<Keys, Action<RdcTreeNode>>
			{
				{
					Keys.Delete,
					delegate(RdcTreeNode node)
					{
						ServerTree.Instance.ConfirmRemove(node, askUser: true);
					}
				},
				{
					Keys.RButton | Keys.MButton | Keys.Back | Keys.Space | Keys.Shift,
					delegate(RdcTreeNode node)
					{
						ServerTree.Instance.ConfirmRemove(node, askUser: false);
					}
				},
				{
					Keys.Return,
					delegate(RdcTreeNode node)
					{
						node.Connect();
					}
				},
				{
					Keys.LButton | Keys.MButton | Keys.Back | Keys.Shift,
					delegate(RdcTreeNode node)
					{
						ServerBase serverBase = node as ServerBase;
						bool allConnected;
						if (serverBase != null)
						{
							allConnected = serverBase.IsConnected;
						}
						else
						{
							(node as GroupBase).AnyOrAllConnected(out bool _, out allConnected);
						}
						if (!allConnected)
						{
							node.DoConnectAs();
						}
					}
				},
				{
					Keys.LButton | Keys.MButton | Keys.Back | Keys.Alt,
					delegate(RdcTreeNode node)
					{
						if (node.HasProperties)
						{
							node.DoPropertiesDialog();
						}
					}
				},
				{
					(Keys)131140,
					delegate(RdcTreeNode node)
					{
						MenuHelper.AddFavorite(node);
					}
				}
			};
		}

		public static MainForm Create()
		{
			MainForm mainForm2 = Program.TheForm = new MainForm();
			if (mainForm2.Initialize())
			{
				return mainForm2;
			}
			return null;
		}

		private bool Initialize()
		{
			_allowSizeChanged = true;
			SuspendLayout();
			InitComp();
			try
			{
				RdpClient.Initialize(this);
			}
			catch
			{
				FormTools.ErrorDialog("RDCMan在初始化期间遇到错误。 可能有两个原因：注册了不兼容的mstscax.dll版本，或者根本没有注册。 请参阅帮助文件以获取详细信息。");
				return false;
			}
			CreateMainMenu();
			SetMainMenuVisibility();
			SetTitle();
			base.VisibleChanged += OnVisibleChanged;
			this.ScaleAndLayout();
			return true;
		}

		public void SetTitle()
		{
			TreeNode selectedNode = ServerTree.Instance.SelectedNode;
			string text = DescriptionText + " v" + VersionText;
			if (selectedNode != null)
			{
				ServerBase serverBase = selectedNode as ServerBase;
				string str;
				if (serverBase != null)
				{
					Server serverNode = serverBase.ServerNode;
					str = serverNode.GetQualifiedNameForUI();
				}
				else
				{
					str = selectedNode.Text;
				}
				text = str + " - " + text;
			}
			Text = text;
		}

		public void RecordLastFocusedServerLabel(ServerLabel label)
		{
			_clientPanel.RecordLastFocusedServerLabel(label);
		}

		public void AddToClientPanel(Control client)
		{
			_clientPanel.Controls.Add(client);
		}

		public void RemoveFromClientPanel(Control client)
		{
			_clientPanel.Controls.Remove(client);
		}

		public void GoToServerTree()
		{
			Program.TheForm.LeaveFullScreen();
			Program.TheForm.EnsureServerTreeVisible();
			ServerTree.Instance.Focus();
		}

		public override void GoFullScreenClient(Server server, bool isTopMostWindow)
		{
			if (!IsFullScreen)
			{
				LockWindowSize(isLocked: false);
				_fullScreenServer = server;
				RemoveFromClientPanel(server.Client.Control);
				_savedControls = new Control[base.Controls.Count];
				base.Controls.CopyTo(_savedControls, 0);
				base.Controls.Clear();
				base.Controls.Add(server.Client.Control);
				base.GoFullScreenClient(server, isTopMostWindow);
			}
		}

		public override bool SwitchFullScreenClient(Server server)
		{
			if (!IsFullScreen || !server.IsClientDocked)
			{
				return false;
			}
			if (server == _fullScreenServer)
			{
				return true;
			}
			RdpClient client = _fullScreenServer.Client;
			RdpClient client2 = server.Client;
			_fullScreenServer.SuspendFullScreenBehavior();
			client.MsRdpClient.FullScreen = false;
			_fullScreenServer.ResumeFullScreenBehavior();
			server.SuspendFullScreenBehavior();
			server.SetNormalView();
			RemoveFromClientPanel(client2.Control);
			base.Controls.Add(client2.Control);
			client2.MsRdpClient.FullScreen = true;
			server.ResumeFullScreenBehavior();
			client2.Control.Bounds = new Rectangle(0, 0, client.Control.Width, client.Control.Height);
			server.GoFullScreen();
			client2.Control.Show();
			client.Control.Hide();
			_fullScreenServer.LeaveFullScreen();
			base.Controls.Remove(client.Control);
			AddToClientPanel(client.Control);
			_fullScreenServer = server;
			return true;
		}

		public void LeaveFullScreen()
		{
			if (_fullScreenServer != null)
			{
				_fullScreenServer.LeaveFullScreen();
			}
		}

		public override void LeaveFullScreenClient(Server server)
		{
			if (IsFullScreen)
			{
				base.Controls.Clear();
				base.Controls.AddRange(_savedControls);
				_savedControls = null;
				AddToClientPanel(server.Client.Control);
				base.LeaveFullScreenClient(server);
				_fullScreenServer = null;
				LockWindowSize();
			}
		}

		public void EnsureServerTreeVisible()
		{
			UpdateServerTreeVisibility(ControlVisibility.Dock);
		}

		private void InitComp()
		{
			bool lockWindowSize = Program.Preferences.LockWindowSize;
			Program.Preferences.LockWindowSize = false;
			_remoteDesktopPanel = new Panel
			{
				Dock = DockStyle.None
			};
			ServerTree.Instance.HideSelection = false;
			ServerTree.Instance.Name = "ServerTree";
			ServerTree.Instance.TabIndex = 0;
			ServerTree.Instance.MouseLeave += SetAutoHideServerTreeTimer;
			ServerTree.Instance.Leave += SetAutoHideServerTreeTimer;
			ServerTree.Instance.LostFocus += AutoHideServerTree;
			ServerTree.Instance.AfterSelect += ServerTree_AfterSelect;
			Server.ConnectionStateChanged += Server_ConnectionStateChange;
			_treeSplitter = new Splitter
			{
				Dock = DockStyle.Left,
				Width = 4,
				MinSize = 10,
				MinExtra = 100
			};
			_treeSplitter.MouseHover += SetAutoShowServerTreeTimer;
			_treeSplitter.MouseLeave += DisableAutoShowTimer;
			_clientPanel = new ClientPanel();
			_remoteDesktopPanel.Controls.Add(ServerTree.Instance, _treeSplitter, _clientPanel);
			_autoSaveTimer = new Timer();
			_autoSaveTimer.Tick += AutoSaveTimerTickHandler;
			SetMainMenuVisibility();
			if (Program.Preferences.ServerTreeWidth > Screen.PrimaryScreen.Bounds.Width)
			{
				ServerTree.Instance.Width = 200;
			}
			else
			{
				ServerTree.Instance.Width = Program.Preferences.ServerTreeWidth;
			}
			ServerTreeLocation = Program.Preferences.ServerTreeLocation;
			ServerTreeVisibility = Program.Preferences.ServerTreeVisibility;
			if (!Program.Preferences.WindowPosition.IsEmpty)
			{
				Screen screen = Screen.FromPoint(Program.Preferences.WindowPosition);
				if (screen.Bounds.Contains(Program.Preferences.WindowPosition))
				{
					base.StartPosition = FormStartPosition.Manual;
					base.Location = Program.Preferences.WindowPosition;
				}
			}
			base.Controls.Add(_remoteDesktopPanel);
			base.Size = Program.Preferences.WindowSize;
			if (Program.Preferences.WindowIsMaximized)
			{
				base.WindowState = FormWindowState.Maximized;
			}
			Program.Preferences.LockWindowSize = lockWindowSize;
			LockWindowSize();
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			base.Icon = new Icon(executingAssembly.GetManifestResourceStream("RdcMan.Resources.app.ico"));
			AssemblyName name = executingAssembly.GetName();
			VersionText = name.Version.Major + "." + name.Version.Minor;
			BuildText = name.Version.Build + "." + name.Version.Revision;
			if (!WinTrust.VerifyEmbeddedSignature(executingAssembly.Location))
			{
				BuildText += "    FOR INTERNAL MICROSOFT USE ONLY";
				IsInternalVersion = true;
			}
			object[] customAttributes = executingAssembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), inherit: false);
			if (customAttributes.Length > 0)
			{
				VersionText += (customAttributes[0] as AssemblyConfigurationAttribute).Configuration;
			}
			customAttributes = executingAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), inherit: false);
			DescriptionText = (customAttributes[0] as AssemblyDescriptionAttribute).Description;
			ServerTree.Instance.Init(executingAssembly);
			_serverTreeAutoHideTimer = new Timer();
			_serverTreeAutoHideTimer.Tick += ServerTreeAutoHideTimerTick;
		}

		private void Server_ConnectionStateChange(ConnectionStateChangedEventArgs args)
		{
		}

		private void ServerTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
		}

		protected override void LayoutContent()
		{
			int width = base.ClientSize.Width;
			int height = base.ClientSize.Height;
			int num = (!Program.Preferences.HideMainMenu) ? _menuPanel.Height : 0;
			_menuPanel.Width = width;
			height -= num;
			ServerTree.Instance.Height = height;
			_remoteDesktopPanel.Bounds = new Rectangle(0, num, width, height);
		}

		private void AutoSaveTimerTickHandler(object sender, EventArgs e)
		{
			if (!_areShuttingDown)
			{
				RdgFile.AutoSave();
				Program.Preferences.Save();
			}
		}

		private void ServerTreeAutoHideTimerTick(object sender, EventArgs e)
		{
			if (Program.Preferences.ServerTreeVisibility == ControlVisibility.AutoHide)
			{
				if (!ServerTree.Instance.Visible)
				{
					_serverTreeAutoHideTimer.Stop();
					UpdateServerTreeVisibility(ControlVisibility.Dock);
					return;
				}
				Rectangle rectangle = ServerTree.Instance.RectangleToScreen(ServerTree.Instance.ClientRectangle);
				rectangle.Inflate(48, 48);
				if (!rectangle.Contains(Control.MousePosition))
				{
					_serverTreeAutoHideTimer.Stop();
					UpdateServerTreeVisibility(ControlVisibility.Hide);
				}
			}
			else
			{
				_serverTreeAutoHideTimer.Stop();
			}
		}

		private void AutoHideServerTree(object sender, EventArgs e)
		{
			if (Program.Preferences.ServerTreeVisibility != 0 && ServerTree.Instance.Visible)
			{
				_serverTreeAutoHideTimer.Stop();
				UpdateServerTreeVisibility(ControlVisibility.Hide);
			}
		}

		private void SetAutoShowServerTreeTimer(object sender, EventArgs e)
		{
			if (Program.Preferences.ServerTreeVisibility == ControlVisibility.AutoHide && !ServerTree.Instance.Visible)
			{
				_serverTreeAutoHideTimer.Interval = Program.Preferences.ServerTreeAutoHidePopUpDelay + 1;
				_serverTreeAutoHideTimer.Start();
			}
		}

		private void DisableAutoShowTimer(object sender, EventArgs e)
		{
			if (Program.Preferences.ServerTreeVisibility == ControlVisibility.AutoHide && !ServerTree.Instance.Visible)
			{
				_serverTreeAutoHideTimer.Stop();
			}
		}

		private void SetAutoHideServerTreeTimer(object sender, EventArgs e)
		{
			if (Program.Preferences.ServerTreeVisibility == ControlVisibility.AutoHide && ServerTree.Instance.Visible)
			{
				_serverTreeAutoHideTimer.Interval = 250;
				_serverTreeAutoHideTimer.Start();
			}
		}

		protected void CreateMainMenu()
		{
			ToolStripMenuItem toolStripMenuItem = _menuStrip.Add("文件", MenuNames.File);
			toolStripMenuItem.DropDownItems.Add(new DelegateMenuItem("新建", MenuNames.FileNew, "Ctrl+N", delegate
			{
				OnFileNew();
			}));
			toolStripMenuItem.DropDownItems.Add(new DelegateMenuItem("打开", MenuNames.FileOpen, "Ctrl+O", delegate
			{
				OnFileOpen();
			}));
			toolStripMenuItem.DropDownItems.Add("-");
			_fileSaveMenuItem = new DelegateMenuItem("", MenuNames.FileSave, "Ctrl+S", delegate
			{
				OnFileSave();
			});
			toolStripMenuItem.DropDownItems.Add(_fileSaveMenuItem);
			_fileSaveAsMenuItem = new DelegateMenuItem("", MenuNames.FileSaveAs, delegate
			{
				OnFileSaveAs();
			});
			toolStripMenuItem.DropDownItems.Add(_fileSaveAsMenuItem);
			toolStripMenuItem.DropDownItems.Add(new DelegateMenuItem("全部保存", MenuNames.FileSaveAll, delegate
			{
				RdgFile.SaveAll();
			}));
			toolStripMenuItem.DropDownItems.Add("-");
			_fileCloseMenuItem = new DelegateMenuItem("", MenuNames.FileClose, delegate
			{
				OnFileClose();
			});
			toolStripMenuItem.DropDownItems.Add(_fileCloseMenuItem);
			toolStripMenuItem.DropDownItems.Add("-");
			toolStripMenuItem.DropDownItems.Add(new DelegateMenuItem("退出", MenuNames.FileExit, "Alt+F4", delegate
			{
				Program.TheForm.Close();
			}));
			ToolStripMenuItem toolStripMenuItem2 = _menuStrip.Add("编辑", MenuNames.Edit);
			toolStripMenuItem2.DropDownItems.Add(new FileRequiredMenuItem("添加服务器", MenuNames.EditAddServer, "Ctrl+A", delegate
			{
				AddNodeDialogHelper.AddServersDialog();
			}));
			toolStripMenuItem2.DropDownItems.Add(new FileRequiredMenuItem("导入服务器", MenuNames.EditImportServers, delegate
			{
				AddNodeDialogHelper.ImportServersDialog();
			}));
			toolStripMenuItem2.DropDownItems.Add(new FileRequiredMenuItem("添加群组", MenuNames.EditAddGroup, "Ctrl+G", delegate
			{
				AddNodeDialogHelper.AddGroupDialog();
			}));
			toolStripMenuItem2.DropDownItems.Add(new FileRequiredMenuItem("添加智能群组", MenuNames.EditAddSmartGroup, delegate
			{
				AddNodeDialogHelper.AddSmartGroupDialog();
			}));
			toolStripMenuItem2.DropDownItems.Add("-");
			toolStripMenuItem2.DropDownItems.Add(new SelectedNodeMenuItem("删除服务器/群组", MenuNames.EditRemove, "Delete", delegate(RdcTreeNode node)
			{
				ServerTree.Instance.ConfirmRemove(node, askUser: true);
			}));
			toolStripMenuItem2.DropDownItems.Add("-");
			toolStripMenuItem2.DropDownItems.Add(new FileRequiredMenuItem("查找", MenuNames.EditFind, "Ctrl+F", delegate
			{
				MenuHelper.FindServers();
			}));
			toolStripMenuItem2.DropDownItems.Add("-");
			toolStripMenuItem2.DropDownItems.Add(new SelectedNodeMenuItem<ServerBase>("添加到收藏夹", MenuNames.EditAddToFavorites, "Ctrl+D", delegate
			{
				MenuHelper.AddFavorite(ServerTree.Instance.SelectedNode as RdcTreeNode);
			}));
			toolStripMenuItem2.DropDownItems.Add("-");
			_editPropertiesMenuItem = new SelectedNodeMenuItem("属性", MenuNames.EditProperties, "Alt+Enter", delegate(RdcTreeNode node)
			{
				node.DoPropertiesDialog();
			});
			toolStripMenuItem2.DropDownItems.Add(_editPropertiesMenuItem);
			ToolStripMenuItem toolStripMenuItem3 = _menuStrip.Add("会话", MenuNames.Session);
			_sessionConnectMenuItem = new SelectedNodeMenuItem("连接", MenuNames.SessionConnect, "Enter", delegate(RdcTreeNode node)
			{
				node.Connect();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionConnectMenuItem);
			_sessionConnectAsMenuItem = new SelectedNodeMenuItem("连接为", MenuNames.SessionConnectAs, "Shift+Enter", delegate(RdcTreeNode node)
			{
				node.DoConnectAs();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionConnectAsMenuItem);
			_sessionReconnectMenuItem = new SelectedNodeMenuItem("重新连接", MenuNames.SessionReconnect, delegate(RdcTreeNode node)
			{
				node.Reconnect();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionReconnectMenuItem);
			toolStripMenuItem3.DropDownItems.Add("-");
			_sessionSendKeysMenuItem = toolStripMenuItem3.DropDownItems.Add("发送按键", MenuNames.SessionSendKeys);
			MenuHelper.AddSendKeysMenuItems(_sessionSendKeysMenuItem, () => ServerTree.Instance.SelectedNode as ServerBase);
			if (RdpClient.SupportsRemoteSessionActions)
			{
				_sessionRemoteActionsMenuItem = toolStripMenuItem3.DropDownItems.Add("远程动作", MenuNames.SessionRemoteActions);
				MenuHelper.AddRemoteActionsMenuItems(_sessionRemoteActionsMenuItem, () => ServerTree.Instance.SelectedNode as ServerBase);
				toolStripMenuItem3.DropDownItems.Add(_sessionRemoteActionsMenuItem);
			}
			toolStripMenuItem3.DropDownItems.Add("-");
			_sessionDisconnectMenuItem = new SelectedNodeMenuItem("断开连接", MenuNames.SessionDisconnect, delegate(RdcTreeNode node)
			{
				node.Disconnect();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionDisconnectMenuItem);
			toolStripMenuItem3.DropDownItems.Add("-");
			_sessionLogOffMenuItem = new SelectedNodeMenuItem("注销", MenuNames.SessionLogOff, delegate(RdcTreeNode node)
			{
				node.LogOff();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionLogOffMenuItem);
			_sessionListSessionsMenuItem = new SelectedNodeMenuItem<ServerBase>("会话列表", MenuNames.SessionListSessions, delegate(ServerBase server)
			{
				Program.ShowForm(new ListSessionsForm(server));
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionListSessionsMenuItem);
			toolStripMenuItem3.DropDownItems.Add("-");
			_sessionFullScreenMenuItem = new SelectedNodeMenuItem<ServerBase>("全屏", MenuNames.SessionFullScreen, delegate(ServerBase server)
			{
				server.GoFullScreen();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionFullScreenMenuItem);
			_sessionUndockMenuItem = new SelectedNodeMenuItem<ServerBase>("取消停靠", MenuNames.SessionUndock, delegate(ServerBase server)
			{
				server.Undock();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionUndockMenuItem);
			_sessionUndockAndConnectMenuItem = new SelectedNodeMenuItem<ServerBase>("取消停靠并连接", MenuNames.SessionUndockAndConnect, delegate(ServerBase server)
			{
				server.Undock();
				server.Connect();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionUndockAndConnectMenuItem);
			_sessionDockMenuItem = new SelectedNodeMenuItem<ServerBase>("停靠", MenuNames.SessionDock, delegate(ServerBase server)
			{
				server.Dock();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionDockMenuItem);
			toolStripMenuItem3.DropDownItems.Add("-");
			toolStripMenuItem3.DropDownItems.Add(new DelegateMenuItem("连接到...", MenuNames.SessionConnectTo, "Ctrl+Q", delegate
			{
				MenuHelper.ConnectTo();
			}));
			toolStripMenuItem3.DropDownItems.Add("-");
			_sessionScreenCaptureMenuItem = new SelectedNodeMenuItem<ServerBase>("屏幕截图", MenuNames.SessionScreenCapture, delegate(ServerBase server)
			{
				server.ScreenCapture();
			});
			toolStripMenuItem3.DropDownItems.Add(_sessionScreenCaptureMenuItem);
			ToolStripMenuItem toolStripMenuItem4 = _menuStrip.Add("视图", MenuNames.View);
			ToolStripMenuItem toolStripMenuItem5 = toolStripMenuItem4.DropDownItems.Add("排序", MenuNames.ViewSortOrder);
			ToolStripMenuItem toolStripMenuItem6 = toolStripMenuItem5.DropDownItems.Add("群组", MenuNames.None);
			toolStripMenuItem6.DropDownItems.Add(new SortGroupsCheckedMenuItem("名称", SortOrder.ByName));
			toolStripMenuItem6.DropDownItems.Add(new SortGroupsCheckedMenuItem("无排序", SortOrder.None));
			ToolStripMenuItem toolStripMenuItem7 = toolStripMenuItem5.DropDownItems.Add("服务器", MenuNames.None);
			toolStripMenuItem7.DropDownItems.Add(new SortServersCheckedMenuItem("状态", SortOrder.ByStatus));
			toolStripMenuItem7.DropDownItems.Add(new SortServersCheckedMenuItem("名称", SortOrder.ByName));
			toolStripMenuItem7.DropDownItems.Add(new SortServersCheckedMenuItem("不排序", SortOrder.None));
			ToolStripMenuItem toolStripMenuItem8 = toolStripMenuItem4.DropDownItems.Add("服务器树位置", MenuNames.ViewServerTreeLocation);
			toolStripMenuItem8.DropDownItems.Add(new ServerTreeLocationMenuItem("靠左", DockStyle.Left));
			toolStripMenuItem8.DropDownItems.Add(new ServerTreeLocationMenuItem("靠右", DockStyle.Right));
			ToolStripMenuItem toolStripMenuItem9 = toolStripMenuItem4.DropDownItems.Add("服务器树可见性", MenuNames.ViewServerTreeVisibility);
			toolStripMenuItem9.DropDownItems.Add(new ServerTreeVisibilityMenuItem("停靠", ControlVisibility.Dock));
			toolStripMenuItem9.DropDownItems.Add(new ServerTreeVisibilityMenuItem("自动隐藏", ControlVisibility.AutoHide));
			toolStripMenuItem9.DropDownItems.Add(new ServerTreeVisibilityMenuItem("隐藏", ControlVisibility.Hide));
			toolStripMenuItem4.DropDownItems.Add("-");
			ToolStripMenuItem toolStripMenuItem10 = toolStripMenuItem4.DropDownItems.Add("内置组", MenuNames.ViewBuiltInGroups);
			foreach (IBuiltInVirtualGroup builtInVirtualGroup in Program.BuiltInVirtualGroups)
			{
				if (builtInVirtualGroup.IsVisibilityConfigurable)
				{
					toolStripMenuItem10.DropDownItems.Add(new BuiltInVirtualGroupCheckedMenuItem(builtInVirtualGroup));
				}
			}
			toolStripMenuItem4.DropDownItems.Add("-");
			toolStripMenuItem4.DropDownItems.Add(new DelegateCheckedMenuItem("锁定窗口大小", MenuNames.ViewLockWindowSize, () => Program.Preferences.LockWindowSize, delegate(bool isChecked)
			{
				Program.Preferences.LockWindowSize = isChecked;
				LockWindowSize();
			}));
			ToolStripMenuItem toolStripMenuItem11 = new ToolStripMenuItem("分辨率");
			Size[] stockSizes = SizeHelper.StockSizes;
			foreach (Size size in stockSizes)
			{
				ClientSizeCheckedMenuItem value = new ClientSizeCheckedMenuItem(this, size);
				toolStripMenuItem11.DropDownItems.Add(value);
			}
			toolStripMenuItem11.DropDownItems.Add(new CustomClientSizeCheckedMenuItem(this, "自定义"));
			toolStripMenuItem11.DropDownItems.Add(new SelectedNodeMenuItem<ServerBase>("适应远程桌面大小", MenuNames.None, delegate(ServerBase node)
			{
				SetClientSize(node.IsConnected ? node.ServerNode.Client.DesktopSize : node.RemoteDesktopSettings.DesktopSize.Value);
			}));
			toolStripMenuItem4.DropDownItems.Add(toolStripMenuItem11);
			RemoteDesktopsMenuItem value2 = new RemoteDesktopsMenuItem();
			_menuStrip.Items.Add(value2);
			ToolStripMenuItem toolStripMenuItem12 = _menuStrip.Add("工具", MenuNames.Tools);
			toolStripMenuItem12.DropDownItems.Add(new DelegateMenuItem("选项", MenuNames.ToolsOptions, delegate
			{
				MenuHelper.ShowGlobalOptionsDialog();
			}));
			ToolStripMenuItem toolStripMenuItem13 = _menuStrip.Add("&Help", MenuNames.Help);
			toolStripMenuItem13.DropDownItems.Add(new DelegateMenuItem("&Usage", MenuNames.HelpUsage, delegate
			{
				Program.Usage();
			}));
			toolStripMenuItem13.DropDownItems.Add("-");
			toolStripMenuItem13.DropDownItems.Add(new DelegateMenuItem("&About...", MenuNames.HelpAbout, delegate
			{
				OnHelpAbout();
			}));
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (!_menuStrip.IsActive)
			{
				if (Shortcuts.TryGetValue(keyData, out Action value))
				{
					value();
					return true;
				}
				RdcTreeNode selectedNode = GetSelectedNode();
				if (selectedNode != null && SelectedNodeShortcuts.TryGetValue(keyData, out Action<RdcTreeNode> value2))
				{
					value2(selectedNode);
					return true;
				}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		public RdcTreeNode GetSelectedNode()
		{
			return _clientPanel.GetSelectedNode(base.ActiveControl) ?? (ServerTree.Instance.SelectedNode as RdcTreeNode);
		}

		public override Size GetClientSize()
		{
			Size result = new Size(_clientPanel.Width, base.ClientSize.Height);
			result.Height -= ((!Program.Preferences.HideMainMenu) ? _menuPanel.Height : 0);
			return result;
		}

		public void ShowGroup(GroupBase group)
		{
			_clientPanel.ShowGroup(group);
		}

		public void HideGroup(GroupBase group)
		{
			_clientPanel.HideGroup(group);
		}

		private bool AnyActive()
		{
			if (ConnectedGroup.Instance.Nodes.Count > 0)
			{
				return FormTools.YesNoDialog("存在活动连接, 确定关闭?") != DialogResult.Yes;
			}
			return false;
		}

		private SaveResult DoExit()
		{
			if (_areShuttingDown)
			{
				return SaveResult.Save;
			}
			if (AnyActive())
			{
				return SaveResult.Cancel;
			}
			foreach (TreeNode node in ServerTree.Instance.Nodes)
			{
				FileGroup fileGroup = node as FileGroup;
				if (fileGroup != null)
				{
					SaveResult saveResult = RdgFile.ShouldSaveFile(fileGroup);
					switch (saveResult)
					{
					case SaveResult.Cancel:
						return saveResult;
					default:
						if (RdgFile.DoSaveWithRetry(fileGroup) == SaveResult.Cancel)
						{
							return SaveResult.Cancel;
						}
						break;
					case SaveResult.NoSave:
						break;
					}
				}
			}
			_areShuttingDown = true;
			_serverTreeAutoHideTimer.Stop();
			_autoSaveTimer.Stop();
			Hide();
			Program.Preferences.WindowIsMaximized = (base.WindowState == FormWindowState.Maximized);
			Rectangle rectangle = (base.WindowState == FormWindowState.Normal) ? base.Bounds : base.RestoreBounds;
			Program.Preferences.WindowPosition = new Point(rectangle.Left, rectangle.Top);
			Program.Preferences.WindowSize = new Size(rectangle.Width, rectangle.Height);
			Program.Preferences.ServerTreeWidth = ServerTree.Instance.Width;
			Program.Preferences.NeedToSave = true;
			Program.Preferences.Save();
			Program.PluginAction(delegate(IPlugin p)
			{
				p.Shutdown();
			});
			using (Helpers.Timer("destroying sessions"))
			{
				ServerTree.Instance.SelectedNode = null;
				ServerTree.Instance.Operation(OperationBehavior.SuspendSelect | OperationBehavior.SuspendSort | OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate
				{
					ServerTree.Instance.Nodes.VisitNodes(delegate(RdcTreeNode node)
					{
						try
						{
							(node as Server)?.OnRemoving();
						}
						catch
						{
						}
					});
					ServerTree.Instance.Nodes.Clear();
				});
			}
			return SaveResult.Save;
		}

		public override void SetClientSize(Size size)
		{
			LockWindowSize(isLocked: false);
			int num = size.Width;
			int height = size.Height;
			if (ServerTreeVisibility == ControlVisibility.Dock)
			{
				num += ServerTree.Instance.Width + _treeSplitter.Width;
			}
			else if (ServerTreeVisibility == ControlVisibility.AutoHide)
			{
				num += _treeSplitter.Width;
			}
			height += ((!Program.Preferences.HideMainMenu) ? _menuPanel.Height : 0);
			base.ClientSize = new Size(num, height);
			LockWindowSize();
		}

		public void LockWindowSize()
		{
			LockWindowSize(Program.Preferences.LockWindowSize);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (_allowSizeChanged)
			{
				LayoutContent();
				Program.Preferences.NeedToSave = true;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = (DoExit() == SaveResult.Cancel);
		}

		private void OnFileNew()
		{
			FileGroup fileGroup = RdgFile.NewFile();
			if (fileGroup != null)
			{
				ServerTree.Instance.SelectedNode = fileGroup;
				Program.Preferences.NeedToSave = true;
			}
		}

		private void OnFileClose()
		{
			FileGroup selectedFile = ServerTree.Instance.GetSelectedFile();
			if (selectedFile != null)
			{
				DoFileClose(selectedFile);
			}
		}

		public void DoFileClose(FileGroup file)
		{
			RdgFile.CloseFileGroup(file);
			Program.Preferences.NeedToSave = true;
			Program.Preferences.Save();
		}

		private void OnFileOpen()
		{
			FileGroup fileGroup = RdgFile.OpenFile();
			if (fileGroup != null)
			{
				ServerTree.Instance.SelectedNode = fileGroup;
				Program.Preferences.NeedToSave = true;
				Program.Preferences.Save();
			}
		}

		private void OnFileSave()
		{
			FileGroup selectedFile = ServerTree.Instance.GetSelectedFile();
			if (selectedFile != null)
			{
				DoFileSave(selectedFile);
			}
		}

		public void DoFileSave(FileGroup file)
		{
			RdgFile.SaveFileGroup(file);
		}

		private void OnFileSaveAs()
		{
			FileGroup selectedFile = ServerTree.Instance.GetSelectedFile();
			if (selectedFile != null)
			{
				RdgFile.SaveAs(selectedFile);
			}
		}

		protected override void UpdateMainMenu()
		{
			UpdateMenuItems(_menuStrip.Items);
			RdcTreeNode selectedNode = GetSelectedNode();
			FileGroup fileGroup = (selectedNode != null && !(selectedNode is ServerRef)) ? selectedNode.FileGroup : null;
			if (fileGroup == null)
			{
				_fileSaveMenuItem.Text = "保存";
				_fileSaveMenuItem.Enabled = false;
				_fileSaveAsMenuItem.Text = "另存为";
				_fileSaveAsMenuItem.Enabled = false;
				_fileCloseMenuItem.Text = "帮";
				_fileCloseMenuItem.Enabled = false;
			}
			else
			{
				_fileSaveMenuItem.Text = "保存 " + fileGroup.GetFilename();
				_fileSaveMenuItem.Enabled = true;
				_fileSaveAsMenuItem.Text = "另存 " + fileGroup.GetFilename() + " 为...";
				_fileSaveAsMenuItem.Enabled = true;
				_fileCloseMenuItem.Text = "关闭 " + fileGroup.GetFilename();
				_fileCloseMenuItem.Enabled = true;
			}
			_editPropertiesMenuItem.Enabled = (selectedNode?.HasProperties ?? false);
			ServerBase serverBase = selectedNode as ServerBase;
			if (serverBase != null)
			{
				bool isConnected = serverBase.IsConnected;
				bool isClientFullScreen = serverBase.IsClientFullScreen;
				_sessionConnectMenuItem.Enabled = !isConnected;
				_sessionConnectAsMenuItem.Enabled = !isConnected;
				_sessionReconnectMenuItem.Enabled = isConnected;
				_sessionDisconnectMenuItem.Enabled = isConnected;
				_sessionLogOffMenuItem.Enabled = (!Policies.DisableLogOff && isConnected);
				_sessionSendKeysMenuItem.Enabled = isConnected;
				if (RdpClient.SupportsRemoteSessionActions)
				{
					_sessionRemoteActionsMenuItem.Enabled = isConnected;
				}
				_sessionListSessionsMenuItem.Enabled = true;
				_sessionFullScreenMenuItem.Enabled = (isConnected && !isClientFullScreen);
				_sessionUndockMenuItem.Enabled = (serverBase.IsClientDocked && !isClientFullScreen);
				_sessionUndockAndConnectMenuItem.Enabled = (serverBase.IsClientDocked && !isClientFullScreen && !isConnected);
				_sessionDockMenuItem.Enabled = serverBase.IsClientUndocked;
				Server serverNode = serverBase.ServerNode;
				_sessionScreenCaptureMenuItem.Enabled = (serverNode.ConnectionState == RdpClient.ConnectionState.Connected && serverNode.IsClientDocked && serverNode.Client.Control.Visible);
				return;
			}
			GroupBase groupBase = selectedNode as GroupBase;
			if (groupBase != null)
			{
				groupBase.AnyOrAllConnected(out bool anyConnected, out bool allConnected);
				_sessionConnectMenuItem.Enabled = !allConnected;
				_sessionConnectAsMenuItem.Enabled = !allConnected;
				_sessionReconnectMenuItem.Enabled = anyConnected;
				_sessionDisconnectMenuItem.Enabled = anyConnected;
				_sessionLogOffMenuItem.Enabled = (!Policies.DisableLogOff && anyConnected);
			}
			else
			{
				_sessionConnectMenuItem.Enabled = false;
				_sessionConnectAsMenuItem.Enabled = false;
				_sessionReconnectMenuItem.Enabled = false;
				_sessionDisconnectMenuItem.Enabled = false;
				_sessionLogOffMenuItem.Enabled = false;
			}
			_sessionSendKeysMenuItem.Enabled = false;
			if (RdpClient.SupportsRemoteSessionActions)
			{
				_sessionRemoteActionsMenuItem.Enabled = false;
			}
			_sessionListSessionsMenuItem.Enabled = false;
			_sessionFullScreenMenuItem.Enabled = false;
			_sessionUndockMenuItem.Enabled = false;
			_sessionUndockAndConnectMenuItem.Enabled = false;
			_sessionDockMenuItem.Enabled = false;
			_sessionScreenCaptureMenuItem.Enabled = false;
		}

		private void LockWindowSize(bool isLocked)
		{
			if (isLocked)
			{
				MinimumSize = base.Size;
				MaximumSize = base.Size;
			}
			else
			{
				MinimumSize = new Size(400, 300);
				MaximumSize = new Size(0, 0);
			}
		}

		private void OnHelpAbout()
		{
			using (About about = new About(IsInternalVersion))
			{
				about.ShowDialog();
			}
		}

		public void UpdateAutoSaveTimer()
		{
			if (Program.Preferences.AutoSaveFiles && Program.Preferences.AutoSaveInterval > 0)
			{
				_autoSaveTimer.Interval = Program.Preferences.AutoSaveInterval * 60 * 1000;
				_autoSaveTimer.Start();
			}
			else
			{
				_autoSaveTimer.Stop();
			}
		}

		private void UpdateServerTreeVisibility(ControlVisibility value)
		{
			SuspendLayout();
			if (value == ControlVisibility.Dock)
			{
				ServerTree.Instance.Enabled = true;
				ServerTree.Instance.Show();
				if (Program.Preferences.ServerTreeVisibility != 0)
				{
					ServerTree.Instance.BringToFront();
					if (Program.Preferences.ServerTreeLocation == DockStyle.Right)
					{
						_treeSplitter.Hide();
					}
				}
				else
				{
					_treeSplitter.SendToBack();
					ServerTree.Instance.SendToBack();
					_treeSplitter.Show();
				}
			}
			else
			{
				ServerTree.Instance.Hide();
				ServerTree.Instance.Enabled = false;
				if (Program.Preferences.ServerTreeVisibility != ControlVisibility.AutoHide)
				{
					_treeSplitter.Hide();
				}
				else
				{
					_treeSplitter.Show();
				}
			}
			ResumeLayout();
		}

		private void OnVisibleChanged(object sender, EventArgs e)
		{
			Program.InitializedEvent.Set();
		}

		bool IMainForm.RegisterShortcut(Keys shortcutKey, Action action)
		{
			try
			{
				Shortcuts.Add(shortcutKey, action);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace RdcMan {
	internal class ServerTree : TreeView, IServerTree {
		private class RootNodeGroup : GroupBase {
			public RootNodeGroup() {
				base.Text = "[root]";
			}

			public override void OnRemoving() {
				throw new NotImplementedException();
			}

			public override void DoPropertiesDialog(Form parentForm, string activeTabName) {
				throw new NotImplementedException();
			}

			protected override void InitSettings() { }
		}

		private class InvalidateComparer : IComparer<RdcTreeNode> {
			public int Compare(RdcTreeNode x, RdcTreeNode y) {
				int ordinal = GetOrdinal(x);
				int ordinal2 = GetOrdinal(y);
				return ordinal - ordinal2;
			}

			private int GetOrdinal(RdcTreeNode node) {
				if (node is Server)
					return 9999;

				if (node is ServerRef)
					return 8888;

				return node.GetPathLength();
			}
		}

		//private const int DelayedFocusDelayMilliseconds = 100;

		private static readonly Color NotFocusedForeColor;

		private static readonly Color NotFocusedBackColor;

		private static readonly Color FocusedForeColor;

		private static readonly Color FocusedBackColor;

		private static readonly ImageConstants[,] ImageConstantLookup;

		private int _noSortCounter;

		private int _noSelectCounter;

		private int _noGroupChanged;

		private bool _contextViaMouse;

		private RdcTreeNode _draggedNode;

		private TreeNode _preDragSelectedNode;

		private ServerBase _delayedFocusServer;

		private readonly System.Threading.Timer _delayedFocusTimer;

		private readonly object _delayedFocusSyncObject = new object();

		private readonly GroupBase _rootNode = new RootNodeGroup();

		internal static ServerTree Instance { get; private set; }

		public GroupBase RootNode => _rootNode;

		internal bool SuppressSorting => _noSortCounter > 0;

		internal event Action<ServerChangedEventArgs> ServerChanged;

		internal event Action<GroupChangedEventArgs> GroupChanged;

		static ServerTree() {
			NotFocusedForeColor = Color.FromKnownColor(KnownColor.ControlDark);
			NotFocusedBackColor = Color.White;
			FocusedForeColor = Color.Black;
			FocusedBackColor = Color.White;
			ImageConstantLookup = new ImageConstants[2, 9];
			foreach (ImageConstants item in Helpers.EnumValues<ImageConstants>()) {
				ImageConstantLookup[0, (int)item] = item;
				ImageConstantLookup[1, (int)item] = item;
			}
			ImageConstantLookup[0, 4] = ImageConstants.ConnectedServer;
			ImageConstantLookup[0, 2] = ImageConstants.ConnectingServer;
			ImageConstantLookup[1, 3] = ImageConstants.ConnectedSelectedServer;
			ImageConstantLookup[1, 1] = ImageConstants.ConnectingSelectedServer;
			Instance = new ServerTree();
		}

		private ServerTree() {
			base.BorderStyle = BorderStyle.None;
			AllowDrop = true;
			base.Scrollable = true;
			base.HideSelection = false;
			_delayedFocusTimer = new System.Threading.Timer(delegate {
				CheckDelayedFocusServer();
			}, null, -1, -1);
		}

		internal FileGroup GetSelectedFile() {
			FileGroup result = null;
			TreeNode treeNode = base.SelectedNode;
			if (treeNode != null)
				result = (treeNode as RdcTreeNode).FileGroup;

			return result;
		}

		internal void Operation(OperationBehavior behavior, Action operation) {
			RdcTreeNode rdcTreeNode = base.SelectedNode as RdcTreeNode;
			try {
				if (behavior.HasFlag(OperationBehavior.SuspendUpdate))
					BeginUpdate();

				if (behavior.HasFlag(OperationBehavior.SuspendSort))
					SuspendSort();

				if (behavior.HasFlag(OperationBehavior.SuspendSelect))
					SuspendSelect();

				if (behavior.HasFlag(OperationBehavior.SuspendGroupChanged))
					SuspendGroupChanged();

				if (behavior.HasFlag(OperationBehavior.RestoreSelected))
					base.SelectedNode = null;

				operation();
			}
			finally {
				if (behavior.HasFlag(OperationBehavior.RestoreSelected)) {
					base.SelectedNode = rdcTreeNode;
					Program.TheForm.SetTitle();
				}
				if (behavior.HasFlag(OperationBehavior.SuspendGroupChanged))
					ResumeGroupChanged();

				if (behavior.HasFlag(OperationBehavior.SuspendSelect))
					ResumeSelect();

				if (behavior.HasFlag(OperationBehavior.SuspendSort))
					ResumeSort();

				if (behavior.HasFlag(OperationBehavior.SuspendUpdate))
					EndUpdate();
			}
		}

		internal void UpdateColors() {
			if (Program.Preferences.DimNodesWhenInactive) {
				if (Focused) {
					ForeColor = FocusedForeColor;
					BackColor = FocusedBackColor;
				}
				else {
					ForeColor = NotFocusedForeColor;
					BackColor = NotFocusedBackColor;
				}
			}
			else {
				ForeColor = FocusedForeColor;
				BackColor = FocusedBackColor;
			}
		}

		internal void Init(Assembly myAssembly) {
			base.ImageList = new ImageList();
			base.ImageList.ColorDepth = ColorDepth.Depth8Bit;
			base.ImageList.ImageSize = new Size(16, 16);
			base.ImageList.Images.Add(new Icon(myAssembly.GetManifestResourceStream("Resources.disconnected.ico")));
			base.ImageList.Images.Add(new Icon(myAssembly.GetManifestResourceStream("Resources.connecting.ico")));
			base.ImageList.Images.Add(new Icon(myAssembly.GetManifestResourceStream("Resources.connectingselected.ico")));
			base.ImageList.Images.Add(new Icon(myAssembly.GetManifestResourceStream("Resources.connected.ico")));
			base.ImageList.Images.Add(new Icon(myAssembly.GetManifestResourceStream("Resources.connectedselected.ico")));
			base.ImageList.Images.Add(new Icon(myAssembly.GetManifestResourceStream("Resources.group.ico")));
			base.ImageList.Images.Add(new Icon(myAssembly.GetManifestResourceStream("Resources.smartgroup.ico")));
			base.ImageList.Images.Add(new Icon(myAssembly.GetManifestResourceStream("Resources.app.ico")));
			ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
			contextMenuStrip.Opening += OnContextMenu;
			ContextMenuStrip = contextMenuStrip;
		}

		public void AddNode(RdcTreeNode node, GroupBase parent) {
			if (node == null)
				throw new ArgumentNullException("node");

			if (!(node is ServerBase) && !(node is GroupBase))
				throw new ArgumentOutOfRangeException("node", "节点必须从 ServerBase 或 GroupBase 派生");

			if (parent == null)
				throw new ArgumentNullException("parent");

			if (parent == RootNode)
				base.Nodes.Add(node);
			else {
				parent.Nodes.Add(node);
				this.SortGroup(parent);
			}
			OnGroupChanged(parent, ChangeType.TreeChanged);
		}

		public void RemoveNode(RdcTreeNode node) {
			if (node == null)
				throw new ArgumentNullException("node");

			TreeNode treeNode = base.SelectedNode;
			TreeNodeCollection parentNodes = ((node.Parent != null) ? node.Parent.Nodes : base.Nodes);
			if (treeNode != null) {
				bool inSelectedPath = false;
				(treeNode as RdcTreeNode).VisitNodeAndParents(delegate (RdcTreeNode n) {
					if (n == node)
						inSelectedPath = true;
				});
				if (inSelectedPath) {
					(treeNode as RdcTreeNode).Hide();
					node.Hide();
					treeNode = ((node.Index > 0) ? parentNodes[node.Index - 1] : ((node.Index >= parentNodes.Count - 1) ? node.Parent : parentNodes[node.Index + 1]));
					base.SelectedNode = null;
				}
			}
			Operation(OperationBehavior.RestoreSelected, delegate {
				GroupBase groupBase = node.Parent as GroupBase;
				node.OnRemoving();
				parentNodes.Remove(node);
				if (groupBase != null)
					OnGroupChanged(groupBase, ChangeType.TreeChanged);
			});
			base.SelectedNode = treeNode;
		}

		private void CheckDelayedFocusServer() {
			lock (_delayedFocusSyncObject) {
				if (_delayedFocusServer != null) {
					Program.TheForm.Invoke((MethodInvoker)delegate {
						_delayedFocusServer.FocusConnectedClient();
					});
				}
				_delayedFocusServer = null;
			}
		}

		private void SetDelayedFocusServer(ServerBase server) {
			lock (_delayedFocusSyncObject) {
				_delayedFocusServer = server;
				_delayedFocusTimer.Change(100, -1);
			}
		}

		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus(e);
			UpdateColors();
		}

		protected override void OnLostFocus(EventArgs e) {
			base.OnLostFocus(e);
			UpdateColors();
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			if (e.Button == MouseButtons.Right)
				_contextViaMouse = true;
			else
				base.OnMouseDown(e);
		}

		protected override void OnBeforeSelect(TreeViewCancelEventArgs e) {
			base.OnBeforeSelect(e);
			if (_noSelectCounter <= 0 && base.SelectedNode != null) {
				RdcTreeNode rdcTreeNode = base.SelectedNode as RdcTreeNode;
				if (!(rdcTreeNode is ServerBase serverBase) || serverBase.IsClientUndocked || !serverBase.IsClientFullScreen)
					rdcTreeNode.Hide();
			}
		}

		protected override void OnAfterSelect(TreeViewEventArgs e) {
			if (_noSelectCounter > 0)
				return;

			if (base.SelectedNode is RdcTreeNode rdcTreeNode) {
				if (rdcTreeNode is ServerBase serverBase) {
					if (serverBase.IsClientUndocked || !serverBase.IsClientFullScreen)
						serverBase.ServerNode.SetNormalView();
					if (!Helpers.IsControlKeyPressed && Program.Preferences.FocusOnClick && e.Action == TreeViewAction.ByMouse && serverBase.IsConnected)
						SetDelayedFocusServer(serverBase);
				}
				if (!rdcTreeNode.IsVisible)
					rdcTreeNode.EnsureVisible();

				rdcTreeNode.Show();
			}
			Program.TheForm.SetTitle();
			base.OnAfterSelect(e);
		}

		private void OnContextMenu(object sender, CancelEventArgs e) {
			RdcTreeNode contextNode = base.SelectedNode as RdcTreeNode;
			if (_contextViaMouse) {
				Point point = PointToClient(Control.MousePosition);
				contextNode = GetNodeAt(point.X, point.Y) as RdcTreeNode;
				_contextViaMouse = false;
				this.SelectedNode = contextNode; //右键点击选中
			}
			PopulateNodeContextMenu(ContextMenuStrip, contextNode);
			Program.PluginAction(delegate (IPlugin p) {
				p.OnContextMenu(ContextMenuStrip, contextNode);
			});
			e.Cancel = false;
		}

		private void PopulateNodeContextMenu(ContextMenuStrip menu, RdcTreeNode node) {
			menu.Items.Clear();
			if (node == null) {
				if (AnyOpenedEditableFiles()) {
					menu.Items.Add(new DelegateMenuItem("添加服务器(&A)...", MenuNames.EditAddServer, AddNodeDialogHelper.AddServersDialog));
					menu.Items.Add(new DelegateMenuItem("导入服务器(&I)...", MenuNames.EditImportServers, AddNodeDialogHelper.ImportServersDialog));
					menu.Items.Add("-");
					menu.Items.Add(new DelegateMenuItem("添加组(&G)...", MenuNames.EditAddGroup, AddNodeDialogHelper.AddGroupDialog));
				}
				else {
					ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("请打开或创建文件");
					toolStripMenuItem.Enabled = false;
					menu.Items.Add(toolStripMenuItem);
				}
			}
			else if (node is GroupBase groupBase) {
				groupBase.AnyOrAllConnected(out var anyConnected, out var allConnected);
				ToolStripMenuItem toolStripMenuItem = new DelegateMenuItem("整组连接(&C)", MenuNames.SessionConnect, groupBase.Connect) {
					Enabled = !allConnected
				};
				menu.Items.Add(toolStripMenuItem);
				toolStripMenuItem = new DelegateMenuItem("整组连接为(&A)...", MenuNames.SessionConnectAs, groupBase.DoConnectAs) {
					Enabled = !allConnected
				};
				menu.Items.Add(toolStripMenuItem);
				toolStripMenuItem = new DelegateMenuItem("整组重连(&E)", MenuNames.SessionReconnect, groupBase.Reconnect) {
					Enabled = anyConnected
				};
				menu.Items.Add(toolStripMenuItem);
				menu.Items.Add("-");
				toolStripMenuItem = new DelegateMenuItem("整组断开(&D)", MenuNames.SessionDisconnect, groupBase.Disconnect) {
					Enabled = anyConnected
				};
				menu.Items.Add(toolStripMenuItem);
				menu.Items.Add("-");
				toolStripMenuItem = new DelegateMenuItem("整组注销(&L)", MenuNames.SessionLogOff, groupBase.LogOff);
				toolStripMenuItem.Enabled = !Policies.DisableLogOff && anyConnected;
				menu.Items.Add(toolStripMenuItem);
				menu.Items.Add("-");
				toolStripMenuItem = new DelegateMenuItem("添加服务器(&A)...", MenuNames.EditAddServer, delegate {
					AddNodeDialogHelper.AddServersDialog(node);
				});
				toolStripMenuItem.Enabled = groupBase.CanAddServers();
				menu.Items.Add(toolStripMenuItem);
				toolStripMenuItem = new DelegateMenuItem("添加组(&G)...", MenuNames.EditAddGroup, delegate {
					AddNodeDialogHelper.AddGroupDialog(node);
				});
				toolStripMenuItem.Enabled = groupBase.CanAddGroups();
				menu.Items.Add(toolStripMenuItem);
				toolStripMenuItem = new DelegateMenuItem("添加智能组(&M)...", MenuNames.EditAddSmartGroup, delegate {
					AddNodeDialogHelper.AddSmartGroupDialog(node);
				});
				toolStripMenuItem.Enabled = groupBase.CanAddGroups();
				menu.Items.Add(toolStripMenuItem);
				menu.Items.Add("-");
				FileGroup file = node as FileGroup;
				if (file != null) {
					toolStripMenuItem = new DelegateMenuItem("保存 " + file.GetFilename(), MenuNames.FileSave, delegate {
						Program.TheForm.DoFileSave(file);
					});
					toolStripMenuItem.Enabled = file.AllowEdit(popUI: false);
					menu.Items.Add(toolStripMenuItem);
					menu.Items.Add(new DelegateMenuItem("关闭 " + file.GetFilename(), MenuNames.FileClose, delegate {
						Program.TheForm.DoFileClose(file);
					}));
				}
				else {
					toolStripMenuItem = new DelegateMenuItem("删除服务器(&R)", MenuNames.EditRemoveServers, delegate {
						DoRemoveChildren(node);
					});
					toolStripMenuItem.Enabled = groupBase.CanRemoveChildren();
					menu.Items.Add(toolStripMenuItem);
					toolStripMenuItem = new DelegateMenuItem("删除组(&G)", MenuNames.EditRemove, delegate {
						ConfirmRemove(node, askUser: true);
					});
					toolStripMenuItem.Enabled = node.CanRemove(popUI: false);
					menu.Items.Add(toolStripMenuItem);
				}
				menu.Items.Add("-");
				toolStripMenuItem = new DelegateMenuItem("属性(&P)", MenuNames.EditProperties, node.DoPropertiesDialog);
				toolStripMenuItem.Enabled = node.HasProperties;
				menu.Items.Add(toolStripMenuItem);
			}
			else {
				ServerBase server = node as ServerBase;
				MenuHelper.AddSessionMenuItems(menu, server);
				menu.Items.Add("-");
				MenuHelper.AddDockingMenuItems(menu, server);
				menu.Items.Add("-");
				MenuHelper.AddMaintenanceMenuItems(menu, server);
			}
		}

		public bool AnyOpenedEditableFiles() {
			return base.Nodes.OfType<FileGroup>().Any((FileGroup file) => file.AllowEdit(popUI: false));
		}

		private TreeNode FindNodeInList(TreeNodeCollection nodes, string name) {
			return (from TreeNode node in nodes
					where node.Text == name
					select node).FirstOrDefault();
		}

		public TreeNode FindNodeByName(string name) {
			if (name == RootNode.Text)
				return RootNode;

			string[] array = name.Split(new string[1] { base.PathSeparator }, StringSplitOptions.None);
			TreeNodeCollection treeNodeCollection = base.Nodes;
			TreeNode treeNode = null;
			string[] array2 = array;
			foreach (string name2 in array2) {
				treeNode = FindNodeInList(treeNodeCollection, name2);
				if (treeNode == null)
					break;

				treeNodeCollection = treeNode.Nodes;
			}
			return treeNode;
		}

		public void ConfirmRemove(RdcTreeNode node, bool askUser) {
			if (node.ConfirmRemove(askUser))
				RemoveNode(node);
		}

		private void DoRemoveChildren(RdcTreeNode node) {
			GroupBase groupBase = node as GroupBase;
			if (groupBase.Nodes.Count > 0) {
				DialogResult dialogResult = FormTools.YesNoDialog("删除 " + groupBase.Text + " 组的所有子项？");
				if (dialogResult != DialogResult.Yes)
					return;
			}
			groupBase.RemoveChildren();
		}

		protected override void OnItemDrag(ItemDragEventArgs e) {
			base.OnItemDrag(e);
			RdcTreeNode rdcTreeNode = (_draggedNode = e.Item as RdcTreeNode);
			_preDragSelectedNode = base.SelectedNode;
			DoDragDrop(_draggedNode, DragDropEffects.Move);
		}

		protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e) {
			base.OnQueryContinueDrag(e);
			if ((e.KeyState & 3) == 0) {
				SuspendSelect();
				base.SelectedNode = _preDragSelectedNode;
				_preDragSelectedNode = null;
				ResumeSelect();
			}
		}

		protected override void OnDragOver(DragEventArgs e) {
			base.OnDragOver(e);
			Point pt = PointToClient(new Point(e.X, e.Y));
			if (GetNodeAt(pt) is RdcTreeNode targetNode && _draggedNode.CanDropOnTarget(targetNode)) {
				SuspendSelect();
				base.SelectedNode = targetNode;
				ResumeSelect();
				e.Effect = e.AllowedEffect;
			}
			else {
				SuspendSelect();
				base.SelectedNode = _draggedNode;
				ResumeSelect();
				e.Effect = DragDropEffects.None;
			}
		}

		protected override void OnDragDrop(DragEventArgs e) {
			base.OnDragDrop(e);
			Point pt = PointToClient(new Point(e.X, e.Y));
			if (GetNodeAt(pt) is RdcTreeNode rdcTreeNode) {
				GroupBase groupBase = (rdcTreeNode as GroupBase) ?? (rdcTreeNode.Parent as GroupBase);
				if (groupBase != _draggedNode && groupBase != _draggedNode.Parent)
					MoveNode(_draggedNode, groupBase);
			}
		}

		protected override void OnNodeMouseDoubleClick(TreeNodeMouseClickEventArgs e) {
			base.OnNodeMouseDoubleClick(e);
			if (e.Button == MouseButtons.Left && !Helpers.IsControlKeyPressed && base.SelectedNode is ServerBase serverBase) {
				serverBase.Connect();
				SetDelayedFocusServer(serverBase);
			}
		}

		public void MoveNode(RdcTreeNode node, GroupBase newParent) {
			if (newParent != null && newParent.HandleMove(node))
				return;

			Operation(OperationBehavior.RestoreSelected, delegate {
				if (node.Parent == null) {
					base.Nodes.Remove(node);
					OnGroupChanged(RootNode, ChangeType.TreeChanged);
				}
				else {
					if (node is ServerBase && (node as ServerBase).ServerNode is TemporaryServer temporaryServer)
						node = temporaryServer;

					GroupBase groupBase = node.Parent as GroupBase;
					groupBase.Nodes.Remove(node);
					OnGroupChanged(groupBase, ChangeType.TreeChanged);
				}
				if (newParent == null) {
					base.Nodes.Add(node);
					OnGroupChanged(RootNode, ChangeType.TreeChanged);
				}
				else {
					newParent.Nodes.Add(node);
					OnGroupChanged(newParent, ChangeType.TreeChanged);
				}
				OnNodeChanged(node, ChangeType.TreeChanged);
				if (!node.IsVisible)
					node.EnsureVisible();
			});
		}

		public void OnGroupChanged(GroupBase group, ChangeType changeType) {
			if (_noGroupChanged > 0 || group == null)
				return;

			Log.Write("OnGroupChanged({1}) {0}", group.Text, changeType);
			HashSet<RdcTreeNode> set = new HashSet<RdcTreeNode>();
			if (group == RootNode) {
				if (changeType.HasFlag(ChangeType.PropertyChanged)) {
					base.Nodes.VisitNodes(delegate (RdcTreeNode n) {
						set.Add(n);
					});
				}
			}
			else {
				group.CollectNodesToInvalidate(recurseChildren: true, set);
				group.VisitParents(delegate (RdcTreeNode parent) {
					parent.CollectNodesToInvalidate(recurseChildren: false, set);
				});
			}
			InvalidateNodes(set);
			this.GroupChanged?.Invoke(new GroupChangedEventArgs(group, changeType));
		}

		public void OnNodeChanged(RdcTreeNode node, ChangeType changeType) {
			Log.Write("OnNodeChanged({1}) {0}", node.Text, changeType);
			if (this.SortNode(node) && node.Parent is GroupBase group)
				OnGroupChanged(group, ChangeType.InvalidateUI);

			if (node is GroupBase group2)
				OnGroupChanged(group2, changeType);
			else
				OnServerChanged(node as ServerBase, changeType);

			Program.TheForm.SetTitle();
		}

		private void OnServerChanged(ServerBase serverBase, ChangeType changeType) {
			HashSet<RdcTreeNode> set = new HashSet<RdcTreeNode>();
			serverBase.CollectNodesToInvalidate(recurseChildren: false, set);
			InvalidateNodes(set);
			this.ServerChanged?.Invoke(new ServerChangedEventArgs(serverBase, changeType));
		}

		public void SuspendSelect() {
			Interlocked.Increment(ref _noSelectCounter);
		}

		public void ResumeSelect() {
			Interlocked.Decrement(ref _noSelectCounter);
		}

		public void SuspendSort() {
			Interlocked.Increment(ref _noSortCounter);
		}

		public void ResumeSort() {
			Interlocked.Decrement(ref _noSortCounter);
		}

		public void SuspendGroupChanged() {
			Interlocked.Increment(ref _noGroupChanged);
		}

		public void ResumeGroupChanged() {
			Interlocked.Decrement(ref _noGroupChanged);
		}

		public static ImageConstants TranslateImage(ImageConstants index, bool toSelected) {
			return ImageConstantLookup[toSelected ? 1 : 0, (int)index];
		}

		private void InvalidateNodes(HashSet<RdcTreeNode> set) {
			foreach (RdcTreeNode item in set.OrderByDescending((RdcTreeNode n) => n, new InvalidateComparer())) {
				item.InvalidateNode();
			}
		}
	}
}

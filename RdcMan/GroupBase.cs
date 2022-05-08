using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using RdcMan.Configuration;

namespace RdcMan {
	public abstract class GroupBase : RdcTreeNode {
		//internal const string XmlGroupNameTag = "name";

		//internal const string XmlExpandedTag = "expanded";

		//internal const string XmlCommentTag = "comment";

		internal static int SchemaVersion;

		protected static Dictionary<string, Helpers.ReadXmlDelegate> NodeActions;

		protected static Dictionary<string, Helpers.ReadXmlDelegate> PropertyActions;

		private int _numberOfConnectedServers = -1;

		private int _numberOfServers = -1;

		public bool IsReadOnly { get; protected set; }

		public new GroupDisplaySettings DisplaySettings => base.DisplaySettings as GroupDisplaySettings;

		public new GroupSettings Properties {
			get => base.Properties as GroupSettings;
			set => base.Properties = value;
		}

		internal bool HasServers {
			get => base.Nodes.Count == 0 ? false : base.Nodes[0] is ServerBase;
		}

		internal int NumberOfServers {
			get {
				if (_numberOfServers == -1) {
					_numberOfServers = 0;
					foreach (TreeNode node in base.Nodes) {
						if (node is GroupBase groupBase)
							_numberOfServers += groupBase.NumberOfServers;
						else
							_numberOfServers++;
					}
				}
				return _numberOfServers;
			}
		}

		internal int NumberOfConnectedServers {
			get {
				if (_numberOfConnectedServers == -1) {
					_numberOfConnectedServers = 0;
					foreach (TreeNode node in base.Nodes) {
						if (node is GroupBase groupBase)
							_numberOfConnectedServers += groupBase.NumberOfConnectedServers;
						else if ((node as ServerBase).IsConnected)
							_numberOfConnectedServers++;
					}
				}
				return _numberOfConnectedServers;
			}
		}

		public virtual bool AllowSort => true;

		public bool HasGroups {
			get => base.Nodes.Count > 0 ? base.Nodes[0] is GroupBase : false;
		}

		static GroupBase() {
			NodeActions = new Dictionary<string, Helpers.ReadXmlDelegate> {
				{
					"properties",
					delegate(XmlNode childNode, RdcTreeNode parent, ICollection<string> errors) {
						if (SchemaVersion <= 2)
							(parent as GroupBase).ReadXml(PropertyActions, childNode, errors);
						else
							(parent as GroupBase).ReadXmlSettingsGroup(childNode, errors);
					}
				},
				{
					"server",
					delegate(XmlNode childNode, RdcTreeNode parent, ICollection<string> errors) {
						Server server = Server.Create(childNode, parent as GroupBase, errors);
						LongRunningActionForm.Instance.UpdateStatus(server.Properties.DisplayName.Value);
					}
				},
				{
					"group",
					delegate(XmlNode childNode, RdcTreeNode parent, ICollection<string> errors) {
						GroupBase groupBase2 = Group.Create(childNode, parent as GroupBase, errors);
						LongRunningActionForm.Instance.UpdateStatus(groupBase2.Properties.GroupName.Value);
					}
				},
				{
					"smartGroup",
					delegate(XmlNode childNode, RdcTreeNode parent, ICollection<string> errors) {
						GroupBase groupBase = SmartGroup.Create(childNode, parent as GroupBase, errors);
						LongRunningActionForm.Instance.UpdateStatus(groupBase.Properties.GroupName.Value);
					}
				}
			};
			PropertyActions = new Dictionary<string, Helpers.ReadXmlDelegate> {
				{
					"name",
					delegate(XmlNode childNode, RdcTreeNode node, ICollection<string> errors) {
						(node as GroupBase).Properties.GroupName.Value = childNode.InnerText;
					}
				},
				{
					"expanded",
					delegate(XmlNode childNode, RdcTreeNode node, ICollection<string> errors) {
						bool.TryParse(childNode.InnerText, out var result);
						(node as GroupBase).Properties.Expanded.Value = result;
					}
				},
				{
					"comment",
					delegate(XmlNode childNode, RdcTreeNode node, ICollection<string> errors) {
						(node as GroupBase).Properties.Comment.Value = childNode.InnerText;
					}
				}
			};
			Server.ConnectionStateChanged += Server_ConnectionStateChanged;
		}

		private static void Server_ConnectionStateChanged(ConnectionStateChangedEventArgs args) {
			(args.Server.Parent as GroupBase).OnConnectionStateChange(args.Server);
			args.Server.VisitServerRefs(delegate (ServerRef r) {
				(r.Parent as GroupBase).OnConnectionStateChange(r);
			});
		}

		public virtual bool CanRemoveChildren() {
			return base.Nodes.Count > 0 ? AllowEdit(popUI: false) : false;
		}

		protected override void InitSettings() {
			base.DisplaySettings = new GroupDisplaySettings();
			base.InitSettings();
		}

		internal override void UpdateSettings(NodePropertiesDialog dlg) {
			base.UpdateSettings(dlg);
			base.Text = Properties.GroupName.Value;
		}

		public override void InvalidateNode() {
			_numberOfServers = -1;
			ResetConnectionStatistics(this);
			base.InvalidateNode();
		}

		internal void ResetConnectionStatistics() {
			this.VisitNodeAndParents(delegate (RdcTreeNode group) {
				ResetConnectionStatistics((GroupBase)group);
			});
		}

		private static void ResetConnectionStatistics(GroupBase group) {
			group._numberOfConnectedServers = -1;
		}

		public virtual bool CanAddServers() {
			return CanDropServers();
		}

		public virtual bool CanAddGroups() {
			return CanDropGroups();
		}

		public virtual bool CanDropServers() {
			return !HasGroups ? AllowEdit(popUI: false) : false;
		}

		public virtual bool CanDropGroups() {
			return !HasServers ? AllowEdit(popUI: false) : false;
		}

		public virtual DragDropEffects DropBehavior() {
			return DragDropEffects.Move;
		}

		internal override void Show() {
			Program.TheForm.ShowGroup(this);
		}

		internal override void Hide() {
			Program.TheForm.HideGroup(this);
		}

		internal override void WriteXml(XmlTextWriter tw) {
			Properties.Expanded.Value = base.IsExpanded;
			WriteXmlSettingsGroups(tw);
			foreach (RdcTreeNode node in base.Nodes)
				node.WriteXml(tw);
		}

		public void RemoveChildren() {
			ServerTree.Instance.Operation(OperationBehavior.SuspendSelect | OperationBehavior.SuspendSort | OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate {
				base.Nodes.ForEach(delegate (TreeNode node) {
					(node as RdcTreeNode).OnRemoving();
				});
				base.Nodes.Clear();
			});
			ServerTree.Instance.OnGroupChanged(this, ChangeType.TreeChanged);
		}

		public override void OnRemoving() {
			Hide();
			RemoveChildren();
		}

		public override void Connect() {
			ConnectAs(null, null);
		}

		public override void ConnectAs(LogonCredentials logonSettings, ConnectionSettings connectionSettings) {
			List<ServerBase> allChildren = this.GetAllChildren((ServerBase s) => !s.IsConnected);
			int count = allChildren.Count;
			if (count >= Current.RdcManSection.WarningThresholds.Connect) {
				DialogResult dialogResult = FormTools.YesNoDialog(base.Text + " 组包含 " + count + " 个断开连接的服务器。 确定吗？");
				if (dialogResult != DialogResult.Yes)
					return;
			}
			NodeHelper.ThrottledConnectAs(allChildren, logonSettings, connectionSettings);
		}

		public override void Reconnect() {
			this.GetAllChildren((ServerBase s) => s.IsConnected).ForEach(delegate (ServerBase server) {
				server.Reconnect();
			});
		}

		public override void Disconnect() {
			NodeHelper.ThrottledDisconnect(this.GetAllChildren((ServerBase s) => s.IsConnected));
		}

		public override void LogOff() {
			foreach (ServerBase allChild in this.GetAllChildren((ServerBase s) => s.IsConnected)) {
				allChild.LogOff();
				Thread.Sleep(25);
			}
		}

		private void OnConnectionStateChange(ServerBase server) {
			InheritSettings();
			if (!server.IsConnected) {
				ReconnectServerRef reconnectServerRef = server.ServerNode.FindServerRef<ReconnectServerRef>();
				if (reconnectServerRef != null && reconnectServerRef.NeedToReconnect)
					return;
			}
			bool flag = false;
			if (Program.Preferences.ServerSortOrder == SortOrder.ByStatus)
				flag |= ServerTree.Instance.SortNode(server);
			if (flag | !(server.ServerNode.Parent as GroupBase).DisplaySettings.ShowDisconnectedThumbnails.Value)
				ServerTree.Instance.OnGroupChanged(this, ChangeType.InvalidateUI);
		}

		public override bool CanDropOnTarget(RdcTreeNode targetNode) {
			if (targetNode == this)
				return true;

			GroupBase groupBase = targetNode as GroupBase;
			if (groupBase == null)
				return false;

			if (!groupBase.CanDropGroups())
				return false;

			if (!AllowEdit(popUI: false))
				return false;

			while (groupBase != null) {
				if (groupBase.Parent == this)
					return false;

				groupBase = groupBase.Parent as GroupBase;
			}
			return true;
		}

		public override bool ConfirmRemove(bool askUser) {
			if (!CanRemove(popUI: true))
				return false;

			AnyOrAllConnected(out var anyConnected, out var _);
			if (anyConnected) {
				FormTools.InformationDialog(base.Text + " 组中有活动会话。在删除组之前断开它们。");
				return false;
			}
			if (askUser && base.Nodes.Count > 0) {
				DialogResult dialogResult = FormTools.YesNoDialog("删除组 " + base.Text + " ？");
				if (dialogResult != DialogResult.Yes)
					return false;
			}
			return true;
		}

		public void AnyOrAllConnected(out bool anyConnected, out bool allConnected) {
			bool any = false;
			bool all = true;
			base.Nodes.VisitNodes(delegate (RdcTreeNode node) {
				if (node is ServerBase serverBase) {
					if (serverBase.IsConnected)
						any = true;
					else
						all = false;
				}
			});
			anyConnected = any;
			allConnected = all;
		}

		internal virtual void ReadXml(XmlNode xmlNode, ICollection<string> errors) {
			ReadXml(NodeActions, xmlNode, errors);
		}
	}
}

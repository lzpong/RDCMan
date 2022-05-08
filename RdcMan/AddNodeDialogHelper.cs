using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan {
	public static class AddNodeDialogHelper {
		public static void AddServersDialog() {
			AddServersDialog(ServerTree.Instance.SelectedNode);
		}

		public static void AddServersDialog(TreeNode suggestedParentNode) {
			if (!ServerTree.Instance.AnyOpenedEditableFiles()) {
				NotifyUserFileNeeded();
			}
			else {
				if (!ServerTree.Instance.Nodes.OfType<FileGroup>().Any() && FormTools.YesNoDialog("RDCMan �������Ϸ��������顣 ���������������ӵ������ļ��飬�����޷�����ļ�����κ��顣 �Ƿ������") != DialogResult.Yes) {
					return;
				}
				GroupBase groupBase = GetParentGroupForServerAdd(suggestedParentNode);
				ServerPropertiesDialog dlg = ServerPropertiesDialog.NewAddDialog(groupBase);
				if (dlg == null) {
					FormTools.InformationDialog("RDCMan �������Ϸ��������顣 Ϊ�˽�����ӵ����ļ�����������ɾ����������");
					return;
				}
				using (dlg) {
					if (dlg.ShowDialog() != DialogResult.OK) {
						return;
					}
					groupBase = dlg.PropertiesPage.ParentGroup;
					Server server = dlg.AssociatedNode as Server;
					server.UpdateSettings(dlg);
					ServerTree.Instance.Operation(OperationBehavior.SuspendSort | OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate {
						List<string> expandedServerNames = (dlg.PropertiesPage as ServerPropertiesTabPage).ExpandedServerNames;
						if (expandedServerNames.Count == 1) {
							Server.Create(dlg);
							return;
						}
						foreach (string item in expandedServerNames) {
							Server.Create(item, dlg);
						}
					});
				}
				FinishAddServers(groupBase);
			}
		}

		private static void FinishAddServers(GroupBase parentGroup) {
			ServerTree.Instance.SortGroup(parentGroup);
			ServerTree.Instance.OnGroupChanged(parentGroup, ChangeType.TreeChanged);
			ServerTree.Instance.SelectedNode = parentGroup;
			parentGroup.Expand();
		}

		public static void ImportServersDialog() {
			ImportServersDialog(ServerTree.Instance.SelectedNode);
		}

		public static void ImportServersDialog(TreeNode parentGroup) {
			if (!ServerTree.Instance.AnyOpenedEditableFiles()) {
				NotifyUserFileNeeded();
				return;
			}
			ServerPropertiesDialog dlg = ServerPropertiesDialog.NewImportDialog(GetParentGroupForServerAdd(parentGroup));
			try {
				if (dlg.ShowDialog() != DialogResult.OK) {
					return;
				}
				GroupBase group = dlg.PropertiesPage.ParentGroup;
				ServerTree.Instance.Operation(OperationBehavior.SuspendSort | OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate {
					Server server = dlg.AssociatedNode as Server;
					server.UpdateSettings(dlg);
					foreach (string serverName in (dlg.PropertiesPage as ImportServersPropertiesPage).ExpandedServerNames) {
						Server server2 = (from s in @group.Nodes.OfType<Server>()
										  where s.ServerName == serverName
										  select s).FirstOrDefault();
						if (server2 != null) {
							server2.UpdateFromTemplate(server);
						}
						else {
							Server.Create(serverName, dlg);
						}
					}
				});
				FinishAddServers(group);
			}
			finally {
				if (dlg != null) {
					((IDisposable)dlg).Dispose();
				}
			}
		}

		public static void AddGroupDialog() {
			AddGroupDialog(ServerTree.Instance.SelectedNode);
		}

		public static void AddGroupDialog(TreeNode suggestedParentNode) {
			if (!ServerTree.Instance.AnyOpenedEditableFiles()) {
				NotifyUserFileNeeded();
				return;
			}
			GroupBase parentGroupForGroupAdd = GetParentGroupForGroupAdd(suggestedParentNode);
			GroupPropertiesDialog groupPropertiesDialog = GroupPropertiesDialog.NewAddDialog(parentGroupForGroupAdd);
			if (groupPropertiesDialog == null) {
				FormTools.InformationDialog("RDCMan �������Ϸ��������顣 Ϊ�˽�����ӵ����ļ�����������ɾ����������");
				return;
			}
			using (groupPropertiesDialog) {
				if (groupPropertiesDialog.ShowDialog() == DialogResult.OK) {
					ServerTree.Instance.SelectedNode = Group.Create(groupPropertiesDialog);
				}
			}
		}

		public static void AddSmartGroupDialog() {
			AddSmartGroupDialog(ServerTree.Instance.SelectedNode);
		}

		public static void AddSmartGroupDialog(TreeNode suggestedParentNode) {
			if (!ServerTree.Instance.AnyOpenedEditableFiles()) {
				NotifyUserFileNeeded();
				return;
			}
			GroupBase parentGroupForGroupAdd = GetParentGroupForGroupAdd(suggestedParentNode);
			SmartGroupPropertiesDialog smartGroupPropertiesDialog = SmartGroupPropertiesDialog.NewAddDialog(parentGroupForGroupAdd);
			if (smartGroupPropertiesDialog == null) {
				FormTools.InformationDialog("RDCMan �������Ϸ��������顣 Ϊ�˽�����ӵ����ļ�����������ɾ����������");
				return;
			}
			using (smartGroupPropertiesDialog) {
				if (smartGroupPropertiesDialog.ShowDialog() == DialogResult.OK) {
					ServerTree.Instance.SelectedNode = SmartGroup.Create(smartGroupPropertiesDialog);
				}
			}
		}

		private static GroupBase GetParentGroupForServerAdd(TreeNode node) {
			GroupBase groupBase = null;
			if (node != null) {
				groupBase = (node as GroupBase) ?? (node.Parent as GroupBase);
				while (groupBase != null && !groupBase.CanAddServers()) {
					groupBase = groupBase.Parent as GroupBase;
				}
			}
			return groupBase;
		}

		private static GroupBase GetParentGroupForGroupAdd(TreeNode node) {
			GroupBase groupBase = null;
			if (node != null) {
				groupBase = (node as GroupBase) ?? (node.Parent as GroupBase);
				while (groupBase != null && !groupBase.CanAddGroups()) {
					groupBase = groupBase.Parent as GroupBase;
				}
			}
			return groupBase;
		}

		private static void NotifyUserFileNeeded() {
			FormTools.InformationDialog("����ӷ�����/��֮ǰ�����һ�����еķ�ֻ���ļ� (File->Open) �򴴽�һ�����ļ� (File->New)");
		}
	}
}
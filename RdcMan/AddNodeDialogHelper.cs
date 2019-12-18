using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan
{
	public static class AddNodeDialogHelper
	{
		public static void AddServersDialog()
		{
			AddServersDialog(ServerTree.Instance.SelectedNode);
		}
		const string info1 = "RDCMan不允许混合使用服务器和组。 为了向该文件添加组，您必须首先删除服务器。";
		public static void AddServersDialog(TreeNode suggestedParentNode)
		{
			if (!ServerTree.Instance.AnyOpenedEditableFiles())
			{
				NotifyUserFileNeeded();
			}
			else if (ServerTree.Instance.Nodes.OfType<FileGroup>().Any() || FormTools.YesNoDialog("RDCMan不允许混合使用服务器和组。 如果将服务器添加到顶级组，则将无法将任何组添加到该文件。 继续吗？") == DialogResult.Yes)
			{
				GroupBase groupBase = GetParentGroupForServerAdd(suggestedParentNode);
				ServerPropertiesDialog dlg = ServerPropertiesDialog.NewAddDialog(groupBase);
				if (dlg == null)
				{
					FormTools.InformationDialog(info1);
					return;
				}
				using (dlg)
				{
					if (dlg.ShowDialog() != DialogResult.OK)
					{
						return;
					}
					groupBase = dlg.PropertiesPage.ParentGroup;
					Server server = dlg.AssociatedNode as Server;
					server.UpdateSettings(dlg);
					ServerTree.Instance.Operation(OperationBehavior.SuspendSort | OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate
					{
						List<string> expandedServerNames = (dlg.PropertiesPage as ServerPropertiesTabPage).ExpandedServerNames;
						if (expandedServerNames.Count == 1)
						{
							Server.Create(dlg);
						}
						else
						{
							foreach (string item in expandedServerNames)
							{
								Server.Create(item, dlg);
							}
						}
					});
				}
				FinishAddServers(groupBase);
			}
		}

		private static void FinishAddServers(GroupBase parentGroup)
		{
			ServerTree.Instance.SortGroup(parentGroup);
			ServerTree.Instance.OnGroupChanged(parentGroup, ChangeType.TreeChanged);
			ServerTree.Instance.SelectedNode = parentGroup;
			parentGroup.Expand();
		}

		public static void ImportServersDialog()
		{
			ImportServersDialog(ServerTree.Instance.SelectedNode);
		}

		public static void ImportServersDialog(TreeNode parentGroup)
		{
			if (!ServerTree.Instance.AnyOpenedEditableFiles())
			{
				NotifyUserFileNeeded();
				return;
			}
			ServerPropertiesDialog dlg = ServerPropertiesDialog.NewImportDialog(GetParentGroupForServerAdd(parentGroup));
			try
			{
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					GroupBase group = dlg.PropertiesPage.ParentGroup;
					ServerTree.Instance.Operation(OperationBehavior.SuspendSort | OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate
					{
						Server server = dlg.AssociatedNode as Server;
						server.UpdateSettings(dlg);
						foreach (string serverName in (dlg.PropertiesPage as ImportServersPropertiesPage).ExpandedServerNames)
						{
							IEnumerable<Server> source = group.Nodes.OfType<Server>();
							Func<Server, bool> predicate = (Server s) => s.ServerName == serverName;
							Server server2 = source.Where(predicate).FirstOrDefault();
							if (server2 != null)
							{
								server2.UpdateFromTemplate(server);
							}
							else
							{
								Server.Create(serverName, dlg);
							}
						}
					});
					FinishAddServers(group);
				}
			}
			finally
			{
				if (dlg != null)
				{
					((IDisposable)dlg).Dispose();
				}
			}
		}

		public static void AddGroupDialog()
		{
			AddGroupDialog(ServerTree.Instance.SelectedNode);
		}

		public static void AddGroupDialog(TreeNode suggestedParentNode)
		{
			if (!ServerTree.Instance.AnyOpenedEditableFiles())
			{
				NotifyUserFileNeeded();
				return;
			}
			GroupBase parentGroupForGroupAdd = GetParentGroupForGroupAdd(suggestedParentNode);
			GroupPropertiesDialog groupPropertiesDialog = GroupPropertiesDialog.NewAddDialog(parentGroupForGroupAdd);
			if (groupPropertiesDialog == null)
			{
				FormTools.InformationDialog(info1);
			}
			else
			{
				using (groupPropertiesDialog)
				{
					if (groupPropertiesDialog.ShowDialog() == DialogResult.OK)
					{
						ServerTree.Instance.SelectedNode = Group.Create(groupPropertiesDialog);
					}
				}
			}
		}

		public static void AddSmartGroupDialog()
		{
			AddSmartGroupDialog(ServerTree.Instance.SelectedNode);
		}

		public static void AddSmartGroupDialog(TreeNode suggestedParentNode)
		{
			if (!ServerTree.Instance.AnyOpenedEditableFiles())
			{
				NotifyUserFileNeeded();
				return;
			}
			GroupBase parentGroupForGroupAdd = GetParentGroupForGroupAdd(suggestedParentNode);
			SmartGroupPropertiesDialog smartGroupPropertiesDialog = SmartGroupPropertiesDialog.NewAddDialog(parentGroupForGroupAdd);
			if (smartGroupPropertiesDialog == null)
			{
				FormTools.InformationDialog(info1);
			}
			else
			{
				using (smartGroupPropertiesDialog)
				{
					if (smartGroupPropertiesDialog.ShowDialog() == DialogResult.OK)
					{
						ServerTree.Instance.SelectedNode = SmartGroup.Create(smartGroupPropertiesDialog);
					}
				}
			}
		}

		private static GroupBase GetParentGroupForServerAdd(TreeNode node)
		{
			GroupBase groupBase = null;
			if (node != null)
			{
				groupBase = ((node as GroupBase) ?? (node.Parent as GroupBase));
				while (groupBase != null && !groupBase.CanAddServers())
				{
					groupBase = (groupBase.Parent as GroupBase);
				}
			}
			return groupBase;
		}

		private static GroupBase GetParentGroupForGroupAdd(TreeNode node)
		{
			GroupBase groupBase = null;
			if (node != null)
			{
				groupBase = ((node as GroupBase) ?? (node.Parent as GroupBase));
				while (groupBase != null && !groupBase.CanAddGroups())
				{
					groupBase = (groupBase.Parent as GroupBase);
				}
			}
			return groupBase;
		}

		private static void NotifyUserFileNeeded()
		{
			FormTools.InformationDialog("在添加服务器/组之前，请打开一个现有的非只读文件（File.Open）或创建一个新文件（File.New）。");
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan
{
	internal static class SortExtensions
	{
		private class ServerTreeSortComparer : IComparer<TreeNode>
		{
			private SortOrder _sortOrder;

			public ServerTreeSortComparer(SortOrder sortOrder)
			{
				_sortOrder = sortOrder;
			}

			public int Compare(TreeNode treeNode1, TreeNode treeNode2)
			{
				if (_sortOrder == SortOrder.ByStatus)
				{
					ImageConstants imageConstants = ServerTree.TranslateImage((ImageConstants)treeNode1.ImageIndex, toSelected: false);
					ImageConstants imageConstants2 = ServerTree.TranslateImage((ImageConstants)treeNode2.ImageIndex, toSelected: false);
					int num = imageConstants2 - imageConstants;
					if (num != 0)
					{
						return num;
					}
				}
				return Helpers.NaturalCompare(treeNode1.Text, treeNode2.Text);
			}
		}

		public static void SortBuiltinGroups(this ServerTree tree)
		{
			if (!tree.SuppressSorting)
			{
				List<TreeNode> list = tree.Nodes.OfType<IBuiltInVirtualGroup>().Cast<TreeNode>().ToList();
				tree.Nodes.SortAndRebuildNodeList(list, SortOrder.ByName);
			}
		}

		public static void SortAllNodes(this ServerTree tree)
		{
			using (Helpers.Timer("sorting all nodes"))
			{
				tree.Operation(OperationBehavior.RestoreSelected, delegate
				{
					tree.SortHelper(tree.Nodes, recurse: true);
					tree.SortBuiltinGroups();
				});
			}
		}

		public static void SortRoot(this ServerTree tree)
		{
			tree.Operation((OperationBehavior)21, delegate
			{
				tree.SortHelper(tree.Nodes, recurse: false);
				tree.SortBuiltinGroups();
			});
		}

		public static bool SortGroup(this ServerTree tree, GroupBase group)
		{
			return tree.SortGroup(group, recurse: false);
		}

		public static bool SortGroup(this ServerTree tree, GroupBase group, bool recurse)
		{
			bool result = false;
			if (group.AllowSort)
			{
				result = tree.SortHelper(group.Nodes, recurse);
			}
			return result;
		}

		public static bool SortNode(this ServerTree tree, RdcTreeNode node)
		{
			GroupBase groupBase = node.Parent as GroupBase;
			if (groupBase != null)
			{
				return tree.SortGroup(groupBase);
			}
			if (node.Parent == null)
			{
				tree.SortRoot();
			}
			return false;
		}

		private static bool SortHelper(this ServerTree tree, TreeNodeCollection nodes, bool recurse)
		{
			if (tree.SuppressSorting)
			{
				return false;
			}
			bool anyChanged = false;
			tree.Operation((OperationBehavior)21, delegate
			{
				anyChanged = tree.SortNodes(nodes, recurse);
			});
			return anyChanged;
		}

		private static bool SortNodes(this ServerTree tree, TreeNodeCollection nodes, bool recurse)
		{
			List<TreeNode> list = new List<TreeNode>(nodes.Count);
			List<TreeNode> list2 = new List<TreeNode>(nodes.Count);
			foreach (TreeNode node in nodes)
			{
				if (node is ServerBase)
				{
					list2.Add(node);
				}
				else
				{
					list.Add(node);
				}
			}
			bool flag = false;
			if (recurse)
			{
				foreach (GroupBase item in from g in list.OfType<GroupBase>()
					where g.AllowSort
					select g)
				{
					flag |= tree.SortNodes(item.Nodes, recurse: true);
				}
			}
			flag |= nodes.SortAndRebuildNodeList(list, Program.Preferences.GroupSortOrder);
			return flag | nodes.SortAndRebuildNodeList(list2, Program.Preferences.ServerSortOrder);
		}

		private static bool SortAndRebuildNodeList(this TreeNodeCollection nodes, List<TreeNode> list, SortOrder sortOrder)
		{
			if (list.Count == 0 || sortOrder == SortOrder.None)
			{
				return false;
			}
			list.Sort(new ServerTreeSortComparer(sortOrder));
			TreeNode treeNode = nodes[0];
			bool result = false;
			foreach (TreeNode item in list)
			{
				if (item == treeNode)
				{
					treeNode = treeNode.NextNode;
				}
				else
				{
					result = true;
					nodes.Remove(item);
					nodes.Insert(treeNode.Index, item);
				}
			}
			return result;
		}
	}
}

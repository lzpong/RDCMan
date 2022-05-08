using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Win32;

namespace RdcMan {
	internal static class NodeHelper {
		private const int ThrottledConnectDelayInMilliseconds = 125;

		private const int ThrottledDisconnectDelayInMilliseconds = 25;

		public static TreeNodeCollection GetParentNodes(this TreeNode node) {
			TreeNode parent = node.Parent;
			return parent != null ? parent.Nodes : node.TreeView.Nodes;
		}

		public static void VisitNodes(this RdcTreeNode node, Action<RdcTreeNode> callback) {
			callback(node);
			node.Nodes.VisitNodes(callback);
		}

		public static void VisitNodes(this ICollection nodes, Action<RdcTreeNode> callback) {
			foreach (RdcTreeNode node in nodes) {
				node.VisitNodes(callback);
			}
		}

		public static bool VisitNodes(this RdcTreeNode node, Func<RdcTreeNode, NodeVisitorResult> callback) {
			return callback(node) switch {
				NodeVisitorResult.NoRecurse => true,
				NodeVisitorResult.Break => false,
				_ => node.Nodes.VisitNodes(callback),
			};
		}

		public static bool VisitNodes(this ICollection nodes, Func<RdcTreeNode, NodeVisitorResult> callback) {
			foreach (RdcTreeNode node in nodes) {
				if (!node.VisitNodes(callback))
					return false;
			}
			return true;
		}

		public static void VisitNodeAndParents(this RdcTreeNode node, Action<RdcTreeNode> callback) {
			callback(node);
			node.VisitParents(callback);
		}

		public static void VisitParents(this RdcTreeNode node, Action<RdcTreeNode> callback) {
			for (RdcTreeNode rdcTreeNode = node.Parent as RdcTreeNode; rdcTreeNode != null; rdcTreeNode = rdcTreeNode.Parent as RdcTreeNode) {
				callback(rdcTreeNode);
			}
		}

		public static void VisitNodeAndParents(this RdcTreeNode node, Func<RdcTreeNode, NodeVisitorResult> callback) {
			if (callback(node) == NodeVisitorResult.Continue)
				node.VisitParents(callback);
		}

		public static void VisitParents(this RdcTreeNode node, Func<RdcTreeNode, NodeVisitorResult> callback) {
			RdcTreeNode rdcTreeNode = node.Parent as RdcTreeNode;
			while (rdcTreeNode != null && callback(rdcTreeNode) == NodeVisitorResult.Continue) {
				rdcTreeNode = rdcTreeNode.Parent as RdcTreeNode;
			}
		}

		public static List<TNode> GetAllChildren<TNode>(this RdcTreeNode parent, Predicate<TNode> predicate) where TNode : RdcTreeNode {
			List<TNode> children = new List<TNode>(parent.Nodes.Count);
			parent.VisitNodes(delegate (RdcTreeNode child) {
				if (child is TNode val && predicate(val))
					children.Add(val);
			});
			return children;
		}

		public static void AnyOrAllConnected(IEnumerable<ServerBase> servers, out bool anyConnected, out bool allConnected) {
			anyConnected = false;
			allConnected = true;
			foreach (ServerBase server in servers) {
				if (server.IsConnected)
					anyConnected = true;
				else
					allConnected = false;
			}
		}

		public static void SelectNewActiveConnection(bool selectPrevious) {
			List<ServerBase> connectedServers = new List<ServerBase>();
			ConnectedGroup.Instance.Nodes.VisitNodes(delegate (RdcTreeNode node) {
				connectedServers.Add(node as ServerBase);
			});
			connectedServers.Sort((ServerBase a, ServerBase b) => (b as ConnectedServerRef).LastFocusTime.CompareTo((a as ConnectedServerRef).LastFocusTime));
			ServerBase serverBase = null;
			if (selectPrevious)
				serverBase = connectedServers[Math.Min(1, connectedServers.Count - 1)];
			else {
				using SelectActiveServerForm selectActiveServerForm = new SelectActiveServerForm(connectedServers);
				if (selectActiveServerForm.ShowDialog() == DialogResult.OK) {
					SelectActiveServerForm.SelectedObject selected = selectActiveServerForm.Selected;
					switch (selected.Operation) {
						case SelectActiveServerForm.Operation.MinimizeWindow: {
							IntPtr window = User.GetWindow(Program.TheForm.Handle, 2u);
							User.SetForegroundWindow(window);
							Program.TheForm.WindowState = FormWindowState.Minimized;
							break;
						}
						case SelectActiveServerForm.Operation.SelectServer:
							serverBase = selected.Server;
							break;
						case SelectActiveServerForm.Operation.SelectTree:
							Program.TheForm.GoToServerTree();
							break;
					}
				}
			}
			if (serverBase != null) {
				Program.TheForm.SwitchFullScreenClient(serverBase.ServerNode);
				ServerTree.Instance.SelectedNode = serverBase;
				serverBase.Focus();
			}
		}

		public static void ThrottledConnect(IEnumerable<ServerBase> servers) {
			ThrottledConnectAs(servers, null, null);
		}

		public static void ThrottledConnect(IEnumerable<ServerBase> servers, Action<ServerBase> postConnectAction) {
			ThrottledConnectAs(servers, null, null, postConnectAction);
		}

		public static void ThrottledConnectAs(IEnumerable<ServerBase> servers, LogonCredentials logonCredentials, ConnectionSettings connectionSettings) {
			ThrottledConnectAs(servers, logonCredentials, connectionSettings, delegate {
			});
		}

		public static void ThrottledConnectAs(IEnumerable<ServerBase> servers, LogonCredentials logonCredentials, ConnectionSettings connectionSettings, Action<ServerBase> postConnectAction) {
			ThrottledOperation(servers, new RdpClient.ConnectionState[2] {
				RdpClient.ConnectionState.Connected,
				RdpClient.ConnectionState.Disconnected
			}, delegate (ServerBase server) {
				server.ConnectAs(logonCredentials, connectionSettings);
				postConnectAction(server);
			}, ThrottledConnectDelayInMilliseconds);
		}

		public static void ThrottledDisconnect(IEnumerable<ServerBase> servers) {
			ThrottledOperation(servers, new RdpClient.ConnectionState[1], delegate (ServerBase server) {
				server.Disconnect();
			}, ThrottledDisconnectDelayInMilliseconds);
		}

		private static void ThrottledOperation(IEnumerable<ServerBase> servers, IEnumerable<RdpClient.ConnectionState> completionStates, Action<ServerBase> operation, int delayInMilliseconds) {
			List<ServerBase> serversList = servers.ToList();
			if (serversList.Count == 0)
				return;

			ThreadPool.QueueUserWorkItem(delegate {
				using ThrottledOperation throttledOperation = new ThrottledOperation(serversList, completionStates, delegate {
					ServerTree.Instance.SuspendSort();
				}, delegate (ServerBase server) {
					Program.TheForm.Invoke((MethodInvoker)delegate {
						operation(server);
					});
				}, delayInMilliseconds, delegate {
					Program.TheForm.Invoke((MethodInvoker)delegate {
						ServerTree.Instance.ResumeSort();
						ServerTree.Instance.SortAllNodes();
					});
				});
				throttledOperation.Execute();
			});
		}
	}
}

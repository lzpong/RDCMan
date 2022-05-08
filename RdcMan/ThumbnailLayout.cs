using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan {
	internal class ThumbnailLayout : IDisposable, IEquatable<ThumbnailLayout> {
		private class LayoutComparer : IComparer<ServerLabel> {
			public int Compare(ServerLabel label1, ServerLabel label2) {
				Server server = label1.Server;
				Server server2 = label2.Server;
				server.InheritSettings();
				server2.InheritSettings();
				int value = server.DisplaySettings.ThumbnailScale.Value;
				int value2 = server2.DisplaySettings.ThumbnailScale.Value;
				int num = value2 - value;
				if (num != 0)
					return num;

				List<TreeNode> path = label1.AssociatedNode.GetPath();
				List<TreeNode> path2 = label2.AssociatedNode.GetPath();
				int num2 = Math.Min(path.Count, path2.Count);
				for (int i = 0; i < num2; i++) {
					num = path[i].Index - path2[i].Index;
					if (num != 0)
						return num;
				}
				return num;
			}
		}

		public int[] ServerTileX;

		public int[] ServerTileY;

		public int[,] ServerLayoutToIndex;

		public ServerLabel[] LabelArray;

		private int[] _tabIndexToServerIndex;

		private Rectangle[] _thumbnailAbsoluteBounds;

		private bool[] _isServerPositionComputed;

		private int _maxNodeIndex;

		private bool _disposed;

		public int FocusedServerIndex { get; set; }

		public int[] TabIndexToServerIndex {
			get {
				EnsureTabIndex();
				return _tabIndexToServerIndex;
			}
		}

		public int NodeCount => LabelArray.Length;

		public int LowestTileY { get; private set; }

		public GroupBase Group { get; private set; }

		public bool IsServerPositionComputed(int index) {
			return _isServerPositionComputed[index];
		}

		public ThumbnailLayout(GroupBase group) {
			Group = group;
		}

		~ThumbnailLayout() {
			Dispose(disposing: false);
		}

		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (_disposed)
				return;

			if (disposing) {
				LabelArray.ForEach(delegate (ServerLabel l) {
					l.Dispose();
				});
				LabelArray = null;
			}
			_disposed = true;
		}

		public override string ToString() {
			return "{0} ({1} ¸ö·þÎñÆ÷)".InvariantFormat(Group.Text, NodeCount);
		}

		public void Compute(int numAcross) {
			using (Helpers.Timer("computing thumbnail layout")) {
				List<ServerLabel> list = CreateThumbnailList();
				LabelArray = list.ToArray();
			}
			if (NodeCount == 0)
				return;

			using (Helpers.Timer("sorting {0} thumbnails", NodeCount)) {
				Array.Sort(LabelArray, new LayoutComparer());
			}
			SetThumbnailIndex();
			FocusedServerIndex = 1;
			_maxNodeIndex = NodeCount + 1;
			int num = 0;
			List<List<ServerLabel>> list2 = new List<List<ServerLabel>>();
			List<ServerLabel> list3 = null;
			ServerLabel[] labelArray = LabelArray;
			int value;
			foreach (ServerLabel serverLabel in labelArray) {
				value = serverLabel.Server.DisplaySettings.ThumbnailScale.Value;
				if (list3 != null && num == value) {
					list3.Add(serverLabel);
					continue;
				}
				list3 = new List<ServerLabel>();
				list3.Add(serverLabel);
				list2.Add(list3);
				num = value;
			}
			value = LabelArray[0].Server.DisplaySettings.ThumbnailScale.Value;
			ServerLayoutToIndex = new int[NodeCount * value, Math.Max(numAcross, value)];
			_isServerPositionComputed = new bool[_maxNodeIndex];
			_thumbnailAbsoluteBounds = new Rectangle[_maxNodeIndex];
			ServerTileX = new int[_maxNodeIndex];
			ServerTileY = new int[_maxNodeIndex];
			using (Helpers.Timer("laying out {0} thumbnails", NodeCount)) {
				int num2 = 0;
				int num3 = 0;
				while (list2.Count > 0) {
					bool flag = false;
					int num4 = -1;
					for (int j = 0; j < list2.Count; j++) {
						list3 = list2[j];
						ServerLabel serverLabel2 = list3[0];
						value = serverLabel2.Server.DisplaySettings.ThumbnailScale.Value;
						if (num4 != -1 && value > num4)
							break;

						bool flag2 = false;
						if (num2 == 0 || num2 + value <= numAcross) {
							int num5 = Math.Min(value, numAcross);
							bool flag3 = true;
							for (int k = 0; k < num5; k++) {
								for (int l = 0; l < value; l++) {
									if (ServerLayoutToIndex[num3 + l, num2 + k] != 0) {
										num4 = k;
										flag3 = false;
										break;
									}
								}
								if (!flag3)
									break;
							}
							if (flag3)
								flag2 = true;
						}
						if (!flag2)
							continue;

						int thumbnailIndex = serverLabel2.ThumbnailIndex;
						for (int m = 0; m < value; m++) {
							for (int n = 0; n < value; n++) {
								ServerLayoutToIndex[num3 + n, num2 + m] = thumbnailIndex;
							}
						}
						ServerTileX[thumbnailIndex] = num2;
						ServerTileY[thumbnailIndex] = num3;
						LowestTileY = Math.Max(LowestTileY, num3 + value - 1);
						flag = true;
						list3.Remove(serverLabel2);
						if (list3.Count == 0)
							list2.Remove(list3);

						num2 += value;
						if (num2 >= numAcross) {
							num2 = 0;
							num3++;
						}
						break;
					}
					if (!flag && ++num2 >= numAcross) {
						num2 = 0;
						num3++;
					}
				}
			}
		}

		public void SetThumbnailIndex() {
			int num = 0;
			ServerLabel[] labelArray = LabelArray;
			foreach (ServerLabel serverLabel in labelArray) {
				num = (serverLabel.ThumbnailIndex = num + 1);
			}
		}

		public void SetThumbnailAbsoluteBounds(int serverIndex, int x, int y, int width, int height) {
			_thumbnailAbsoluteBounds[serverIndex] = new Rectangle(x, y, width, height);
			_isServerPositionComputed[serverIndex] = true;
		}

		public Rectangle GetThumbnailAbsoluteBounds(int serverIndex) {
			return _thumbnailAbsoluteBounds[serverIndex];
		}

		public void EnsureTabIndex() {
			if (_tabIndexToServerIndex != null) {
				return;
			}
			int upperBound = ServerLayoutToIndex.GetUpperBound(1);
			int lowestTileY = LowestTileY;
			int num = 1;
			_tabIndexToServerIndex = new int[_maxNodeIndex];
			for (int i = 0; i <= lowestTileY; i++) {
				ServerLabel serverLabel;
				for (int j = 0; j <= upperBound; j += serverLabel.Server.DisplaySettings.ThumbnailScale.Value) {
					int num2 = ServerLayoutToIndex[i, j];
					if (num2 == 0)
						break;

					serverLabel = LabelArray[num2 - 1];
					if (ServerTileY[num2] == i) {
						_tabIndexToServerIndex[num] = serverLabel.ThumbnailIndex;
						serverLabel.TabIndex = num++;
					}
				}
			}
		}

		private List<ServerLabel> CreateThumbnailList() {
			List<ServerLabel> labelList = new List<ServerLabel>();
			HashSet<Server> set = new HashSet<Server>();
			bool useActualNode = Group is VirtualGroup;
			Group.VisitNodes(delegate (RdcTreeNode node) {
				if (node is GroupBase groupBase)
					groupBase.InheritSettings();
				else {
					ServerBase serverBase = node as ServerBase;
					Server serverNode = serverBase.ServerNode;
					if (!set.Contains(serverNode) && serverNode.Parent is GroupBase groupBase2) {
						groupBase2.InheritSettings();
						if (groupBase2.DisplaySettings.ShowDisconnectedThumbnails.Value || serverNode.IsConnected) {
							ServerLabel item = new ServerLabel(useActualNode ? serverBase : serverNode);
							labelList.Add(item);
							set.Add(serverNode);
						}
					}
				}
			});
			return labelList;
		}

		public bool Equals(ThumbnailLayout other) {
			if (Group != other.Group || NodeCount != other.NodeCount)
				return false;

			for (int i = 0; i < NodeCount; i++) {
				if (LabelArray[i].AssociatedNode != other.LabelArray[i].AssociatedNode || ServerTileX[i] != other.ServerTileX[i] || ServerTileY[i] != other.ServerTileY[i])
					return false;
			}
			return true;
		}
	}
}

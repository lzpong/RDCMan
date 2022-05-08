using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Win32;

namespace RdcMan {
	internal class ClientPanel : Control {
		private static readonly int ThumbnailLabelHeight;

		private const int ThumbnailHorzSpace = 8;

		private const int ThumbnailVertSpace = 6;

		private readonly Dictionary<TreeNode, ThumbnailLayout> _layoutHash;

		private readonly Dictionary<TreeNode, int> _groupScrollPosition;

		private ThumbnailLayout _layout;

		private Size _savedSize;

		private int _thumbnailUnitWidth;

		private int _thumbnailUnitHeight;

		private bool[] _thumbnailDrawn;

		private readonly VScrollBar _verticalScrollBar;

		private int UnitHeight => Math.Max(_thumbnailUnitHeight + ThumbnailLabelHeight + ThumbnailVertSpace, 1);

		static ClientPanel() {
			ThumbnailLabelHeight = ServerLabel.Height;
		}

		public ClientPanel() {
			base.TabStop = false;
			_verticalScrollBar = new VScrollBar {
				Dock = DockStyle.Right,
				TabStop = false,
				Visible = false
			};
			_verticalScrollBar.Scroll += OnScroll;
			base.Controls.Add(_verticalScrollBar);
			Dock = DockStyle.Fill;
			DoubleBuffered = false;
			_groupScrollPosition = new Dictionary<TreeNode, int>();
			ServerTree.Instance.GroupChanged += OnGroupChanged;
			ServerTree.Instance.ServerChanged += OnServerChanged;
			_layoutHash = new Dictionary<TreeNode, ThumbnailLayout>();
		}

		private void OnGroupChanged(GroupChangedEventArgs obj) {
			if ((obj.Group == ServerTree.Instance.RootNode && !obj.ChangeType.HasFlag(ChangeType.PropertyChanged))
				|| (obj.Group != ServerTree.Instance.RootNode && !obj.ChangeType.HasFlag(ChangeType.InvalidateUI)))
				return;

			ThumbnailLayout layout = _layout;
			GroupBase groupBase = layout?.Group;
			bool flag = false;
			if (layout != null) {
				ThumbnailLayout thumbnailLayout = CreateThumbnailLayout(groupBase);
				if (!obj.ChangeType.HasFlag(ChangeType.PropertyChanged) && thumbnailLayout.Equals(layout)) {
					Log.Write("布局不变，不重绘");
					thumbnailLayout.Dispose();
					UpdateNonLayoutSettings(layout);
				}
				else {
					HideGroup(groupBase);
					layout.Dispose();
					_layoutHash[groupBase] = thumbnailLayout;
					ShowGroup(groupBase);
					flag = true;
				}
			}
			TreeNode group = obj.Group;
			if (group != ServerTree.Instance.RootNode) {
				while (group != null) {
					if (group == groupBase) {
						if (!flag)
							break;
					}
					else if (_layoutHash.TryGetValue(group, out ThumbnailLayout value)) {
						_layoutHash.Remove(group);
						value.Dispose();
					}
					group = group.Parent;
				}
			}
			else
				ResetLayout();

			if (layout == null && ServerTree.Instance.SelectedNode is ServerBase serverBase)
				UpdateNonLayoutSettings(serverBase.ServerNode);
		}

		private void UpdateNonLayoutSettings(ThumbnailLayout shownLayout) {
			shownLayout.LabelArray.ForEach(delegate (ServerLabel l) {
				UpdateNonLayoutSettings(l.Server);
			});
		}

		private void UpdateNonLayoutSettings(Server server) {
			server.SetClientSizeProperties();
			server.EnableDisableClient();
		}

		private void OnServerChanged(ServerChangedEventArgs obj) {
			if (!obj.ChangeType.HasFlag(ChangeType.PropertyChanged))
				return;

			using (Helpers.Timer("thumbnail ServerChanged handler")) {
				Server serverNode = obj.Server.ServerNode;
				foreach (ThumbnailLayout value in _layoutHash.Values) {
					ServerLabel[] labelArray = value.LabelArray;
					foreach (ServerLabel serverLabel in labelArray) {
						if (serverLabel.Server == serverNode)
							serverLabel.CopyServerData();
					}
				}
			}
		}

		public RdcTreeNode GetSelectedNode(Control active) {
			if (active is ServerLabel serverLabel)
				return serverLabel.AssociatedNode;
			return null;
		}

		private void ResetLayout() {
			foreach (ThumbnailLayout item in _layoutHash.Values.Where((ThumbnailLayout l) => l != _layout).ToList()) {
				item.Dispose();
				_layoutHash.Remove(item.Group);
			}
		}

		[Conditional("DEBUG")]
		private void AssertValid() {
			foreach (Control control in base.Controls) { }
		}

		public void ShowGroup(GroupBase group) {
			bool flag = true;
			if (_layout != null && _layout.Group == group)
				flag = false;

			if (!_layoutHash.TryGetValue(group, out _layout)) {
				_layout = CreateThumbnailLayout(group);
				_layoutHash.Add(group, _layout);
			}
			ComputeScrollBarLimits();
			if (flag) {
				if (!_groupScrollPosition.TryGetValue(group, out var value))
					value = 0;
				SetScrollPosition(value);
			}
			_thumbnailDrawn = new bool[_layout.NodeCount];
			DrawThumbnails(group);
		}

		private ThumbnailLayout CreateThumbnailLayout(GroupBase group) {
			using (Helpers.Timer("creating thumbnail layout for {0}", group.Text)) {
				ThumbnailLayout thumbnailLayout = new ThumbnailLayout(group);
				int num;
				if (Program.Preferences.ThumbnailSizeIsInPixels) {
					Size thumbnailSize = Program.Preferences.ThumbnailSize;
					_thumbnailUnitWidth = thumbnailSize.Width;
					_thumbnailUnitHeight = thumbnailSize.Height;
					num = ComputeNumAcross(base.ClientSize.Width, _thumbnailUnitWidth);
				}
				else {
					num = 100 / Program.Preferences.ThumbnailPercentage;
					_thumbnailUnitWidth = (base.ClientSize.Width - _verticalScrollBar.Width - ThumbnailHorzSpace) / num - ThumbnailHorzSpace;
					_thumbnailUnitHeight = (base.ClientSize.Height - ThumbnailVertSpace) / num - ThumbnailLabelHeight - ThumbnailVertSpace;
					Program.Preferences.ThumbnailSize = new Size(_thumbnailUnitWidth, _thumbnailUnitHeight);
				}
				thumbnailLayout.Compute(num);
				return thumbnailLayout;
			}
		}

		public void HideGroup(GroupBase group) {
			if (_layout != null) {
				if (_layout.Group != group)
					return;

				_groupScrollPosition[group] = _verticalScrollBar.Value;
				try {
					ServerLabel serverLabel = Program.TheForm.ActiveControl as ServerLabel;
					ServerLabel[] labelArray = _layout.LabelArray;
					foreach (ServerLabel serverLabel2 in labelArray) {
						if (serverLabel == serverLabel2)
							Program.TheForm.ActiveControl = this;

						serverLabel2.Server.Hide();
						base.Controls.Remove(serverLabel2);
					}
				}
				finally {
					_layout = null;
				}
			}
			_verticalScrollBar.Hide();
		}

		public void ScrollServerIntoView(ServerLabel label) {
			int thumbnailIndex = label.ThumbnailIndex;
			if (!_layout.IsServerPositionComputed(thumbnailIndex))
				ComputeThumbnailPosition(label);

			Rectangle thumbnailAbsoluteBounds = _layout.GetThumbnailAbsoluteBounds(thumbnailIndex);
			int num = thumbnailAbsoluteBounds.Bottom + ServerLabel.Height;
			int num2 = base.ClientSize.Height;
			if (thumbnailAbsoluteBounds.Top < _verticalScrollBar.Value || num > _verticalScrollBar.Value + num2 - 1) {
				int value = _verticalScrollBar.Value;
				int scrollPosition = ((value >= thumbnailAbsoluteBounds.Top) ? thumbnailAbsoluteBounds.Top : (num - num2 + 1));
				SetScrollPosition(scrollPosition);
				DrawAndScroll(value, _verticalScrollBar.Value);
			}
		}

		private void ComputeScrollBarLimits() {
			int unitHeight = UnitHeight;
			int num = (_layout.LowestTileY + 1) * unitHeight;
			int num2 = base.ClientSize.Height;
			int num3 = num - num2;
			if (_verticalScrollBar.Visible = num3 > 0) {
				_verticalScrollBar.LargeChange = num2;
				_verticalScrollBar.SmallChange = unitHeight;
				_verticalScrollBar.Maximum = num;
				SetScrollPosition(_verticalScrollBar.Value);
			}
		}

		private int GetServerHeight(int serverScale) {
			return _thumbnailUnitHeight * serverScale + (ThumbnailLabelHeight + ThumbnailVertSpace) * (serverScale - 1);
		}

		private void SetScrollPosition(int value) {
			_verticalScrollBar.Value = Math.Min(value, _verticalScrollBar.Maximum - _verticalScrollBar.LargeChange + 1);
		}

		private void DrawThumbnails(GroupBase group) {
			if (_layout.NodeCount == 0)
				return;

			using (Helpers.Timer("drawing {0} ({1} thumbnails)", group.Text, _layout.NodeCount)) {
				DrawThumbnails(_verticalScrollBar.Value, _verticalScrollBar.Value, base.ClientSize.Height);
			}
		}

		private void DrawThumbnails(int oldValue, int newValue, int height) {
			foreach (int item in GetUndrawnServersInViewport(newValue, height)) {
				_thumbnailDrawn[item - 1] = true;
				ServerLabel label = _layout.LabelArray[item - 1];
				if (!_layout.IsServerPositionComputed(item))
					ComputeThumbnailPosition(label);

				DrawThumbnail(label, oldValue);
			}
		}

		private void DrawThumbnail(ServerLabel label, int windowTop) {
			Rectangle thumbnailAbsoluteBounds = _layout.GetThumbnailAbsoluteBounds(label.ThumbnailIndex);
			int num = thumbnailAbsoluteBounds.Top - windowTop;
			int top = num + ThumbnailLabelHeight - 1;
			Server server = label.Server;
			server.SetThumbnailView(thumbnailAbsoluteBounds.X, top, thumbnailAbsoluteBounds.Width, thumbnailAbsoluteBounds.Height);
			label.Size = new Size(thumbnailAbsoluteBounds.Width, ThumbnailLabelHeight);
			label.Location = new Point(thumbnailAbsoluteBounds.X, num);
			base.Controls.Add(label);
			label.Show();
			server.Show();
			if (Program.TheForm.ActiveControl == this && label.ThumbnailIndex == _layout.FocusedServerIndex) {
				label.Focus();
				_layout.FocusedServerIndex = 0;
			}
		}

		private unsafe void DrawAndScroll(int oldValue, int newValue) {
			using (Helpers.Timer("scrolling thumbnails {0} => {1}", oldValue, newValue)) {
				Size clientSize = base.ClientSize;
				int num = clientSize.Height;
				DrawThumbnails(oldValue, newValue, num);
				if (oldValue != newValue) {
					Structs.RECT rECT = default(Structs.RECT);
					rECT.top = 0;
					rECT.bottom = clientSize.Height;
					rECT.left = 0;
					rECT.right = _verticalScrollBar.Left - 1;
					Structs.RECT rECT2 = default(Structs.RECT);
					rECT.top = -oldValue;
					rECT.bottom = _verticalScrollBar.Maximum - oldValue;
					rECT.left = 0;
					rECT.right = _verticalScrollBar.Left - 1;
					Structs.RECT* ptr = &rECT;
					Structs.RECT* ptr2 = &rECT2;
					User.ScrollWindowEx(base.Handle, 0, oldValue - newValue, (IntPtr)ptr2, (IntPtr)ptr, (IntPtr)(void*)null, (IntPtr)(void*)null, 7u);
				}
			}
		}

		protected override void OnClientSizeChanged(EventArgs e) {
			if (Program.TheForm.IsFullScreen)
				return;

			Size clientSize = base.ClientSize;
			try {
				TreeNode selectedNode = ServerTree.Instance.SelectedNode;
				if (!(selectedNode is GroupBase groupBase)) {
					if (selectedNode is ServerBase serverBase) {
						if (serverBase.IsThumbnail)
							throw new InvalidOperationException("所选服务器是缩略图");

						serverBase.ServerNode.SetNormalView();
						if (clientSize.Width != _savedSize.Width || clientSize.Height != _savedSize.Height)
							serverBase.ServerNode.Resize();
					}
					ResetLayout();
				}
				else {
					if (_layout == null || _layout.Group != groupBase) {
						return;
					}
					if (Program.Preferences.ThumbnailSizeIsInPixels) {
						int unitWidth = Program.Preferences.ThumbnailSize.Width;
						if (ComputeNumAcross(clientSize.Width, unitWidth) == ComputeNumAcross(_savedSize.Width, unitWidth)) {
							if (_savedSize.Height != clientSize.Height) {
								int value = _verticalScrollBar.Value;
								ComputeScrollBarLimits();
								DrawAndScroll(value, _verticalScrollBar.Value);
							}
							return;
						}
					}
					HideGroup(groupBase);
					ResetLayout();
					ShowGroup(groupBase);
				}
			}
			finally {
				_savedSize = clientSize;
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e) {
			if (ServerTree.Instance.SelectedNode is GroupBase) {
				int value = _verticalScrollBar.Value;
				int val = value - Math.Sign(e.Delta) * _verticalScrollBar.SmallChange;
				SetScrollPosition(Math.Max(0, val));
				DrawAndScroll(value, _verticalScrollBar.Value);
			}
		}

		private int ComputeNumAcross(int totalWidth, int unitWidth) {
			totalWidth -= _verticalScrollBar.Width;
			totalWidth -= ThumbnailHorzSpace;
			return totalWidth / (unitWidth + ThumbnailHorzSpace);
		}

		private void ComputeThumbnailPosition(ServerLabel label) {
			int value = label.Server.DisplaySettings.ThumbnailScale.Value;
			int num = _thumbnailUnitWidth * value + ThumbnailHorzSpace * (value - 1);
			int serverHeight = GetServerHeight(value);
			int thumbnailIndex = label.ThumbnailIndex;
			int num2 = _layout.ServerTileY[thumbnailIndex];
			int num3 = _layout.ServerTileX[thumbnailIndex];
			int num4 = (num3 + 1) * ThumbnailHorzSpace + num3 * _thumbnailUnitWidth;
			int num5 = (num2 + 1) * ThumbnailVertSpace + num2 * (_thumbnailUnitHeight + ThumbnailLabelHeight);
			_layout.SetThumbnailAbsoluteBounds(thumbnailIndex, num4, num5, num, serverHeight);
		}

		private IEnumerable<int> GetUndrawnServersInViewport(int position, int height) {
			HashSet<int> hashSet = new HashSet<int>();
			if (_layout.NodeCount == 0)
				return hashSet;

			int upperBound = _layout.ServerLayoutToIndex.GetUpperBound(1);
			int unitHeight = UnitHeight;
			int num = position / unitHeight;
			int val = (position + height - 1) / unitHeight;
			val = Math.Min(val, _layout.ServerLayoutToIndex.GetUpperBound(0));
			for (int i = num; i <= val; i++) {
				ServerLabel serverLabel;
				for (int j = 0; j <= upperBound; j += serverLabel.Server.DisplaySettings.ThumbnailScale.Value) {
					int num2 = _layout.ServerLayoutToIndex[i, j];
					if (num2 == 0)
						break;

					if (!_thumbnailDrawn[num2 - 1])
						hashSet.Add(num2);

					serverLabel = _layout.LabelArray[num2 - 1];
				}
			}
			return hashSet;
		}

		private void OnScroll(object sender, ScrollEventArgs e) {
			DrawAndScroll(e.OldValue, e.NewValue);
		}

		public void RecordLastFocusedServerLabel(ServerLabel label) {
			_layout.FocusedServerIndex = label.ThumbnailIndex;
		}

		protected override void OnEnter(EventArgs e) {
			bool flag = true;
			base.OnEnter(e);
			if (_layout != null) {
				if (_layout.NodeCount > 0) {
					ServerLabel serverLabel = _layout.LabelArray[_layout.FocusedServerIndex - 1];
					if (serverLabel.Parent == this) {
						serverLabel.Focus();
						flag = false;
					}
				}
			}
			else if (ServerTree.Instance.SelectedNode is ServerBase serverBase) {
				serverBase.Focus();
				flag = false;
			}
			if (flag) {
				Focus();
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			ServerLabel serverLabel = Program.TheForm.ActiveControl as ServerLabel;
			if (serverLabel == null) {
				if (_layout == null || Program.TheForm.ActiveControl != this)
					return base.ProcessCmdKey(ref msg, keyData);

				serverLabel = _layout.LabelArray[_layout.FocusedServerIndex - 1];
			}
			_layout.EnsureTabIndex();
			int value = serverLabel.Server.DisplaySettings.ThumbnailScale.Value;
			int thumbnailIndex = serverLabel.ThumbnailIndex;
			int num = _layout.ServerTileX[thumbnailIndex];
			int num2 = _layout.ServerTileY[thumbnailIndex];
			int num3 = serverLabel.TabIndex;
			switch (keyData) {
				case Keys.Tab:
				case Keys.Tab | Keys.Shift:
					ServerTree.Instance.Focus();
					return true;
				case Keys.Left:
					if (--num3 == 0)
						return true;

					thumbnailIndex = _layout.TabIndexToServerIndex[num3];
					num = _layout.ServerTileX[thumbnailIndex];
					num2 = _layout.ServerTileY[thumbnailIndex];
					break;
				case Keys.Right:
					if (++num3 > _layout.NodeCount)
						return true;

					thumbnailIndex = _layout.TabIndexToServerIndex[num3];
					num = _layout.ServerTileX[thumbnailIndex];
					num2 = _layout.ServerTileY[thumbnailIndex];
					break;
				case Keys.Up:
					if (--num2 < 0)
						return true;
					break;
				case Keys.Down:
					num2 += value;
					if (num2 > _layout.LowestTileY)
						return true;
					break;
				case Keys.Home:
					num = 0;
					num2 = 0;
					break;
				case Keys.End:
					num = _layout.ServerLayoutToIndex.GetUpperBound(1);
					num2 = _layout.LowestTileY;
					break;
				case Keys.Prior:
					if (num2 == 0)
						return true;

					int val3 = (int)Math.Floor((double)serverLabel.Top / (double)UnitHeight);
					int val4 = base.Height / UnitHeight;
					int num5 = Math.Max(val3, val4);
					num2 = Math.Max(0, num2 - num5);
					break;
				case Keys.Next:
					if (num2 == _layout.LowestTileY)
						return true;

					int val = (int)Math.Floor((double)(serverLabel.Top + GetServerHeight(value)) / (double)UnitHeight);
					int val2 = base.Height / UnitHeight;
					int num4 = Math.Max(val, val2);
					num2 = Math.Min(_layout.LowestTileY, num2 + num4);
					break;

				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
			while (true) {
				thumbnailIndex = _layout.ServerLayoutToIndex[num2, num];
				if (thumbnailIndex != 0)
					break;

				num--;
			}
			serverLabel = _layout.LabelArray[thumbnailIndex - 1];
			ScrollServerIntoView(serverLabel);
			serverLabel.Focus();
			return true;
		}
	}
}

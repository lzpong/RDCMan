using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Win32;

namespace RdcMan
{
	internal class ClientPanel : Control
	{
		private const int ThumbnailHorzSpace = 8;

		private const int ThumbnailVertSpace = 6;

		private static readonly int ThumbnailLabelHeight;

		private Dictionary<TreeNode, ThumbnailLayout> _layoutHash;

		private readonly Dictionary<TreeNode, int> _groupScrollPosition;

		private ThumbnailLayout _layout;

		private Size _savedSize;

		private int _thumbnailUnitWidth;

		private int _thumbnailUnitHeight;

		private bool[] _thumbnailDrawn;

		private readonly VScrollBar _verticalScrollBar;

		private int UnitHeight => _thumbnailUnitHeight + ThumbnailLabelHeight + 6;

		static ClientPanel()
		{
			ThumbnailLabelHeight = ServerLabel.Height;
		}

		public ClientPanel()
		{
			base.TabStop = false;
			_verticalScrollBar = new VScrollBar
			{
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

		private void OnGroupChanged(GroupChangedEventArgs obj)
		{
			if ((obj.Group == ServerTree.Instance.RootNode && !obj.ChangeType.HasFlag(ChangeType.PropertyChanged)) || (obj.Group != ServerTree.Instance.RootNode && !obj.ChangeType.HasFlag(ChangeType.InvalidateUI)))
			{
				return;
			}
			ThumbnailLayout layout = _layout;
			GroupBase groupBase = layout?.Group;
			bool flag = false;
			if (layout != null)
			{
				ThumbnailLayout thumbnailLayout = CreateThumbnailLayout(groupBase);
				if (!obj.ChangeType.HasFlag(ChangeType.PropertyChanged) && thumbnailLayout.Equals(layout))
				{
					Log.Write("布局不变,不重绘");
					thumbnailLayout.Dispose();
					UpdateNonLayoutSettings(layout);
				}
				else
				{
					HideGroup(groupBase);
					layout.Dispose();
					_layoutHash[groupBase] = thumbnailLayout;
					ShowGroup(groupBase);
					flag = true;
				}
			}
			TreeNode treeNode = obj.Group;
			if (treeNode != ServerTree.Instance.RootNode)
			{
				while (treeNode != null)
				{
					ThumbnailLayout value;
					if (treeNode == groupBase)
					{
						if (!flag)
						{
							break;
						}
					}
					else if (_layoutHash.TryGetValue(treeNode, out value))
					{
						_layoutHash.Remove(treeNode);
						value.Dispose();
					}
					treeNode = treeNode.Parent;
				}
			}
			else
			{
				ResetLayout();
			}
			if (layout == null)
			{
				ServerBase serverBase = ServerTree.Instance.SelectedNode as ServerBase;
				if (serverBase != null)
				{
					UpdateNonLayoutSettings(serverBase.ServerNode);
				}
			}
		}

		private void UpdateNonLayoutSettings(ThumbnailLayout shownLayout)
		{
			shownLayout.LabelArray.ForEach(delegate(ServerLabel l)
			{
				UpdateNonLayoutSettings(l.Server);
			});
		}

		private void UpdateNonLayoutSettings(Server server)
		{
			server.SetClientSizeProperties();
			server.EnableDisableClient();
		}

		private void OnServerChanged(ServerChangedEventArgs obj)
		{
			if (obj.ChangeType.HasFlag(ChangeType.PropertyChanged))
			{
				using (Helpers.Timer("thumbnail ServerChanged handler"))
				{
					Server serverNode = obj.Server.ServerNode;
					foreach (ThumbnailLayout value in _layoutHash.Values)
					{
						ServerLabel[] labelArray = value.LabelArray;
						foreach (ServerLabel serverLabel in labelArray)
						{
							if (serverLabel.Server == serverNode)
							{
								serverLabel.CopyServerData();
							}
						}
					}
				}
			}
		}

		public RdcTreeNode GetSelectedNode(Control active)
		{
			return (active as ServerLabel)?.AssociatedNode;
		}

		private void ResetLayout()
		{
			foreach (ThumbnailLayout item in _layoutHash.Values.Where((ThumbnailLayout l) => l != _layout).ToList())
			{
				item.Dispose();
				_layoutHash.Remove(item.Group);
			}
		}

		[Conditional("DEBUG")]
		private void AssertValid()
		{
			foreach (Control control in base.Controls)
			{
				_ = control;
			}
		}

		public void ShowGroup(GroupBase group)
		{
			bool flag = true;
			if (_layout != null && _layout.Group == group)
			{
				flag = false;
			}
			if (!_layoutHash.TryGetValue(group, out _layout))
			{
				_layout = CreateThumbnailLayout(group);
				_layoutHash.Add(group, _layout);
			}
			ComputeScrollBarLimits();
			if (flag)
			{
				if (!_groupScrollPosition.TryGetValue(group, out int value))
				{
					value = 0;
				}
				SetScrollPosition(value);
			}
			_thumbnailDrawn = new bool[_layout.NodeCount];
			DrawThumbnails(group);
		}

		private ThumbnailLayout CreateThumbnailLayout(GroupBase group)
		{
			using (Helpers.Timer("creating thumbnail layout for {0}", group.Text))
			{
				ThumbnailLayout thumbnailLayout = new ThumbnailLayout(group);
				int num;
				if (Program.Preferences.ThumbnailSizeIsInPixels)
				{
					Size thumbnailSize = Program.Preferences.ThumbnailSize;
					_thumbnailUnitWidth = thumbnailSize.Width;
					_thumbnailUnitHeight = thumbnailSize.Height;
					num = ComputeNumAcross(base.ClientSize.Width, _thumbnailUnitWidth);
				}
				else
				{
					num = 100 / Program.Preferences.ThumbnailPercentage;
					_thumbnailUnitWidth = (base.ClientSize.Width - _verticalScrollBar.Width - 8) / num - 8;
					_thumbnailUnitHeight = (base.ClientSize.Height - 6) / num - ThumbnailLabelHeight - 6;
					Program.Preferences.ThumbnailSize = new Size(_thumbnailUnitWidth, _thumbnailUnitHeight);
				}
				thumbnailLayout.Compute(num);
				return thumbnailLayout;
			}
		}

		public void HideGroup(GroupBase group)
		{
			if (_layout != null)
			{
				if (_layout.Group != group)
				{
					return;
				}
				_groupScrollPosition[group] = _verticalScrollBar.Value;
				try
				{
					ServerLabel serverLabel = Program.TheForm.ActiveControl as ServerLabel;
					ServerLabel[] labelArray = _layout.LabelArray;
					foreach (ServerLabel serverLabel2 in labelArray)
					{
						if (serverLabel == serverLabel2)
						{
							Program.TheForm.ActiveControl = this;
						}
						serverLabel2.Server.Hide();
						base.Controls.Remove(serverLabel2);
					}
				}
				finally
				{
					_layout = null;
				}
			}
			_verticalScrollBar.Hide();
		}

		public void ScrollServerIntoView(ServerLabel label)
		{
			int thumbnailIndex = label.ThumbnailIndex;
			if (!_layout.IsServerPositionComputed(thumbnailIndex))
			{
				ComputeThumbnailPosition(label);
			}
			Rectangle thumbnailAbsoluteBounds = _layout.GetThumbnailAbsoluteBounds(thumbnailIndex);
			int num = thumbnailAbsoluteBounds.Bottom + ServerLabel.Height;
			int height = base.ClientSize.Height;
			if (thumbnailAbsoluteBounds.Top < _verticalScrollBar.Value || num > _verticalScrollBar.Value + height - 1)
			{
				int value = _verticalScrollBar.Value;
				int scrollPosition = (value >= thumbnailAbsoluteBounds.Top) ? thumbnailAbsoluteBounds.Top : (num - height + 1);
				SetScrollPosition(scrollPosition);
				DrawAndScroll(value, _verticalScrollBar.Value);
			}
		}

		private void ComputeScrollBarLimits()
		{
			int unitHeight = UnitHeight;
			int num = (_layout.LowestTileY + 1) * unitHeight;
			int height = base.ClientSize.Height;
			int num2 = num - height;
			if (_verticalScrollBar.Visible = (num2 > 0))
			{
				_verticalScrollBar.LargeChange = height;
				_verticalScrollBar.SmallChange = unitHeight;
				_verticalScrollBar.Maximum = num;
				SetScrollPosition(_verticalScrollBar.Value);
			}
		}

		private int GetServerHeight(int serverScale)
		{
			return _thumbnailUnitHeight * serverScale + (ThumbnailLabelHeight + 6) * (serverScale - 1);
		}

		private void SetScrollPosition(int value)
		{
			_verticalScrollBar.Value = Math.Min(value, _verticalScrollBar.Maximum - _verticalScrollBar.LargeChange + 1);
		}

		private void DrawThumbnails(GroupBase group)
		{
			if (_layout.NodeCount != 0)
			{
				using (Helpers.Timer("drawing {0} ({1} thumbnails)", group.Text, _layout.NodeCount))
				{
					DrawThumbnails(_verticalScrollBar.Value, _verticalScrollBar.Value, base.ClientSize.Height);
				}
			}
		}

		private void DrawThumbnails(int oldValue, int newValue, int height)
		{
			foreach (int item in GetUndrawnServersInViewport(newValue, height))
			{
				_thumbnailDrawn[item - 1] = true;
				ServerLabel label = _layout.LabelArray[item - 1];
				if (!_layout.IsServerPositionComputed(item))
				{
					ComputeThumbnailPosition(label);
				}
				DrawThumbnail(label, oldValue);
			}
		}

		private void DrawThumbnail(ServerLabel label, int windowTop)
		{
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
			if (Program.TheForm.ActiveControl == this && label.ThumbnailIndex == _layout.FocusedServerIndex)
			{
				label.Focus();
				_layout.FocusedServerIndex = 0;
			}
		}

		private unsafe void DrawAndScroll(int oldValue, int newValue)
		{
			using (Helpers.Timer("scrolling thumbnails {0} => {1}", oldValue, newValue))
			{
				Size clientSize = base.ClientSize;
				int height = clientSize.Height;
				DrawThumbnails(oldValue, newValue, height);
				if (oldValue != newValue)
				{
					Structs.RECT rECT = default(Structs.RECT);
					rECT.top = 0;
					rECT.bottom = clientSize.Height;
					rECT.left = 0;
					rECT.right = _verticalScrollBar.Left - 1;
					Structs.RECT rECT2 = rECT;
					Structs.RECT rECT3 = default(Structs.RECT);
					rECT3.top = -oldValue;
					rECT3.bottom = _verticalScrollBar.Maximum - oldValue;
					rECT3.left = 0;
					rECT3.right = _verticalScrollBar.Left - 1;
					Structs.RECT rECT4 = rECT3;
					Structs.RECT* value = &rECT2;
					Structs.RECT* value2 = &rECT4;
					User.ScrollWindowEx(base.Handle, 0, oldValue - newValue, (IntPtr)(void*)value2, (IntPtr)(void*)value, (IntPtr)null, (IntPtr)null, 7u);
				}
			}
		}

		protected override void OnClientSizeChanged(EventArgs e)
		{
			if (!Program.TheForm.IsFullScreen)
			{
				Size clientSize = base.ClientSize;
				try
				{
					TreeNode selectedNode = ServerTree.Instance.SelectedNode;
					GroupBase groupBase = selectedNode as GroupBase;
					if (groupBase == null)
					{
						ServerBase serverBase = selectedNode as ServerBase;
						if (serverBase != null)
						{
							if (serverBase.IsThumbnail)
							{
								throw new InvalidOperationException("Selected server is a thumbnail");
							}
							serverBase.ServerNode.SetNormalView();
						}
						ResetLayout();
					}
					else if (_layout != null && _layout.Group == groupBase)
					{
						if (!Program.Preferences.ThumbnailSizeIsInPixels)
						{
							goto IL_00f8;
						}
						int width = Program.Preferences.ThumbnailSize.Width;
						if (ComputeNumAcross(clientSize.Width, width) != ComputeNumAcross(_savedSize.Width, width))
						{
							goto IL_00f8;
						}
						if (_savedSize.Height != clientSize.Height)
						{
							int value = _verticalScrollBar.Value;
							ComputeScrollBarLimits();
							DrawAndScroll(value, _verticalScrollBar.Value);
						}
					}
					goto end_IL_0014;
					IL_00f8:
					HideGroup(groupBase);
					ResetLayout();
					ShowGroup(groupBase);
					end_IL_0014:;
				}
				finally
				{
					_savedSize = clientSize;
				}
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (ServerTree.Instance.SelectedNode is GroupBase)
			{
				int value = _verticalScrollBar.Value;
				int val = value - Math.Sign(e.Delta) * _verticalScrollBar.SmallChange;
				SetScrollPosition(Math.Max(0, val));
				DrawAndScroll(value, _verticalScrollBar.Value);
			}
		}

		private int ComputeNumAcross(int totalWidth, int unitWidth)
		{
			totalWidth -= _verticalScrollBar.Width;
			totalWidth -= 8;
			return totalWidth / (unitWidth + 8);
		}

		private void ComputeThumbnailPosition(ServerLabel label)
		{
			int value = label.Server.DisplaySettings.ThumbnailScale.Value;
			int width = _thumbnailUnitWidth * value + 8 * (value - 1);
			int serverHeight = GetServerHeight(value);
			int thumbnailIndex = label.ThumbnailIndex;
			int num = _layout.ServerTileY[thumbnailIndex];
			int num2 = _layout.ServerTileX[thumbnailIndex];
			int x = (num2 + 1) * 8 + num2 * _thumbnailUnitWidth;
			int y = (num + 1) * 6 + num * (_thumbnailUnitHeight + ThumbnailLabelHeight);
			_layout.SetThumbnailAbsoluteBounds(thumbnailIndex, x, y, width, serverHeight);
		}

		private IEnumerable<int> GetUndrawnServersInViewport(int position, int height)
		{
			HashSet<int> hashSet = new HashSet<int>();
			if (_layout.NodeCount == 0)
			{
				return hashSet;
			}
			int upperBound = _layout.ServerLayoutToIndex.GetUpperBound(1);
			int unitHeight = UnitHeight;
			int num = position / unitHeight;
			int val = (position + height - 1) / unitHeight;
			val = Math.Min(val, _layout.ServerLayoutToIndex.GetUpperBound(0));
			for (int i = num; i <= val; i++)
			{
				ServerLabel serverLabel;
				for (int j = 0; j <= upperBound; j += serverLabel.Server.DisplaySettings.ThumbnailScale.Value)
				{
					int num2 = _layout.ServerLayoutToIndex[i, j];
					if (num2 == 0)
					{
						break;
					}
					if (!_thumbnailDrawn[num2 - 1])
					{
						hashSet.Add(num2);
					}
					serverLabel = _layout.LabelArray[num2 - 1];
				}
			}
			return hashSet;
		}

		private void OnScroll(object sender, ScrollEventArgs e)
		{
			DrawAndScroll(e.OldValue, e.NewValue);
		}

		public void RecordLastFocusedServerLabel(ServerLabel label)
		{
			_layout.FocusedServerIndex = label.ThumbnailIndex;
		}

		protected override void OnEnter(EventArgs e)
		{
			bool flag = true;
			base.OnEnter(e);
			if (_layout != null)
			{
				if (_layout.NodeCount > 0)
				{
					ServerLabel serverLabel = _layout.LabelArray[_layout.FocusedServerIndex - 1];
					if (serverLabel.Parent == this)
					{
						serverLabel.Focus();
						flag = false;
					}
				}
			}
			else
			{
				ServerBase serverBase = ServerTree.Instance.SelectedNode as ServerBase;
				if (serverBase != null)
				{
					serverBase.Focus();
					flag = false;
				}
			}
			if (flag)
			{
				Focus();
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			ServerLabel serverLabel = Program.TheForm.ActiveControl as ServerLabel;
			if (serverLabel == null)
			{
				if (_layout == null || Program.TheForm.ActiveControl != this)
				{
					return base.ProcessCmdKey(ref msg, keyData);
				}
				serverLabel = _layout.LabelArray[_layout.FocusedServerIndex - 1];
			}
			_layout.EnsureTabIndex();
			int value = serverLabel.Server.DisplaySettings.ThumbnailScale.Value;
			int thumbnailIndex = serverLabel.ThumbnailIndex;
			int num = _layout.ServerTileX[thumbnailIndex];
			int num2 = _layout.ServerTileY[thumbnailIndex];
			int tabIndex = serverLabel.TabIndex;
			switch (keyData)
			{
			case Keys.Tab:
			case Keys.LButton | Keys.Back | Keys.Shift:
				ServerTree.Instance.Focus();
				return true;
			case Keys.Left:
				if (--tabIndex == 0)
				{
					return true;
				}
				thumbnailIndex = _layout.TabIndexToServerIndex[tabIndex];
				num = _layout.ServerTileX[thumbnailIndex];
				num2 = _layout.ServerTileY[thumbnailIndex];
				break;
			case Keys.Right:
				if (++tabIndex > _layout.NodeCount)
				{
					return true;
				}
				thumbnailIndex = _layout.TabIndexToServerIndex[tabIndex];
				num = _layout.ServerTileX[thumbnailIndex];
				num2 = _layout.ServerTileY[thumbnailIndex];
				break;
			case Keys.Up:
				if (--num2 < 0)
				{
					return true;
				}
				break;
			case Keys.Down:
				num2 += value;
				if (num2 > _layout.LowestTileY)
				{
					return true;
				}
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
			{
				if (num2 == 0)
				{
					return true;
				}
				int val3 = (int)Math.Floor((double)serverLabel.Top / (double)UnitHeight);
				int val4 = base.Height / UnitHeight;
				int num4 = Math.Max(val3, val4);
				num2 = Math.Max(0, num2 - num4);
				break;
			}
			case Keys.Next:
			{
				if (num2 == _layout.LowestTileY)
				{
					return true;
				}
				int val = (int)Math.Floor((double)(serverLabel.Top + GetServerHeight(value)) / (double)UnitHeight);
				int val2 = base.Height / UnitHeight;
				int num3 = Math.Max(val, val2);
				num2 = Math.Min(_layout.LowestTileY, num2 + num3);
				break;
			}
			default:
				return base.ProcessCmdKey(ref msg, keyData);
			}
			while (true)
			{
				thumbnailIndex = _layout.ServerLayoutToIndex[num2, num];
				if (thumbnailIndex != 0)
				{
					break;
				}
				num--;
			}
			serverLabel = _layout.LabelArray[thumbnailIndex - 1];
			ScrollServerIntoView(serverLabel);
			serverLabel.Focus();
			return true;
		}
	}
}

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan {
	public class GlobalOptionsDialog : TabbedSettingsDialog {
		private class BandwidthItem {
			public string Text;

			public int Flags;

			public BandwidthItem(string text, int flags) {
				Text = text;
				Flags = flags;
			}
		}

		private GroupBox _virtualGroupsGroup;

		private ValueComboBox<ControlVisibility> _treeVisibilityCombo;

		private ValueComboBox<DockStyle> _treeLocationCombo;

		private CheckBox _connectionBarEnabledCheckBox;

		private CheckBox _connectionBarAutoHiddenCheckBox;

		private RdcCheckBox _enablePanningCheckBox;

		private RdcNumericUpDown _panningAccelerationUpDown;

		private GroupBox _casSizeGroup;

		private RadioButton _casCustomRadio;

		private RadioButton _thumbnailPixelsRadio;

		private RadioButton _thumbnailPercentageRadio;

		private TextBox _thumbnailPercentageTextBox;

		private Button _casCustomButton;

		private Button _thumbnailPixelsButton;

		private ValueComboBox<BandwidthItem> _bandwidthComboBox;

		private CheckBox _desktopBackgroundCheckBox;

		private CheckBox _fontSmoothingCheckBox;

		private CheckBox _desktopCompositionCheckBox;

		private CheckBox _windowDragCheckBox;

		private CheckBox _menuAnimationCheckBox;

		private CheckBox _themesCheckBox;

		private CheckBox _persistentBitmapCachingCheckBox;

		private bool _inHandler;

		private readonly BandwidthItem[] _bandwidthItems;

		protected GlobalOptionsDialog(Form parentForm) : base("选项", "确定", parentForm) {
			_bandwidthItems = new BandwidthItem[5]
			{
				new BandwidthItem("Modem (28.8 Kbps)", 15),
				new BandwidthItem("Modem (56 Kbps)", 7),
				new BandwidthItem("Broadband (128 Kpbs - 1.5 Mbps)", 257),
				new BandwidthItem("LAN (10 Mbps or higher)", 384),
				new BandwidthItem("自定义", 0)
			};
			InitializeComponent();
		}

		public static GlobalOptionsDialog New() {
			GlobalOptionsDialog globalOptionsDialog = new GlobalOptionsDialog(Program.TheForm);
			globalOptionsDialog.InitializeControlsFromPreferences();
			return globalOptionsDialog;
		}

		private void InitializeControlsFromPreferences() {
			MainForm theForm = Program.TheForm;
			foreach (CheckBox control in _virtualGroupsGroup.Controls) {
				control.Checked = (control.Tag as IBuiltInVirtualGroup).IsInTree;
			}
			_treeLocationCombo.SelectedValue = Program.TheForm.ServerTreeLocation;
			_treeVisibilityCombo.SelectedValue = Program.TheForm.ServerTreeVisibility;
			_connectionBarEnabledCheckBox.Checked = Program.Preferences.ConnectionBarState != RdpClient.ConnectionBarState.Off;
			_connectionBarAutoHiddenCheckBox.Checked = Program.Preferences.ConnectionBarState == RdpClient.ConnectionBarState.AutoHide;
			_connectionBarAutoHiddenCheckBox.Enabled = _connectionBarEnabledCheckBox.Enabled;
			if (RdpClient.SupportsPanning)
				_panningAccelerationUpDown.Enabled = Program.Preferences.EnablePanning;

			Size clientSize = theForm.GetClientSize();
			RadioButton radioButton = _casSizeGroup.Controls.OfType<RadioButton>().Where(delegate (RadioButton r) {
				Size? size = (Size?)r.Tag;
				Size size2 = clientSize;
				return !size.HasValue ? false : !size.HasValue || size.GetValueOrDefault() == size2;
			}).FirstOrDefault();
			if (radioButton != null)
				radioButton.Checked = true;
			else
				_casCustomRadio.Checked = true;

			_casCustomButton.Text = clientSize.ToFormattedString();
			_thumbnailPixelsButton.Text = Program.Preferences.ThumbnailSize.ToFormattedString();
			_thumbnailPercentageTextBox.Text = Program.Preferences.ThumbnailPercentage.ToString();
			if (Program.Preferences.ThumbnailSizeIsInPixels)
				_thumbnailPixelsRadio.Checked = true;
			else
				_thumbnailPercentageRadio.Checked = true;

			SetBandwidthCheckBoxes(Program.Preferences.PerformanceFlags);
			_persistentBitmapCachingCheckBox.Checked = Program.Preferences.PersistentBitmapCaching;
		}

		public void UpdatePreferences() {
			UpdateSettings();
			MainForm theForm = Program.TheForm;
			foreach (CheckBox control in _virtualGroupsGroup.Controls) {
				(control.Tag as IBuiltInVirtualGroup).IsInTree = control.Checked;
			}
			Program.TheForm.ServerTreeLocation = _treeLocationCombo.SelectedValue;
			Program.TheForm.ServerTreeVisibility = _treeVisibilityCombo.SelectedValue;
			Program.Preferences.ConnectionBarState = (_connectionBarEnabledCheckBox.Checked ? ((!_connectionBarAutoHiddenCheckBox.Checked) ? RdpClient.ConnectionBarState.Pinned : RdpClient.ConnectionBarState.AutoHide) : RdpClient.ConnectionBarState.Off);
			Program.Preferences.PerformanceFlags = ComputeFlagsFromCheckBoxes();
			Program.Preferences.PersistentBitmapCaching = _persistentBitmapCachingCheckBox.Checked;
			string dim = _casCustomButton.Text;
			if (!_casCustomRadio.Checked) {
				dim = (from r in _casSizeGroup.Controls.OfType<RadioButton>()
					   where r.Checked
					   select r).First().Text;
			}
			Size size = SizeHelper.Parse(dim);
			Size clientSize = theForm.GetClientSize();
			if (clientSize != size)
				theForm.SetClientSize(size);

			size = SizeHelper.Parse(_thumbnailPixelsButton.Text);
			Program.Preferences.ThumbnailSize = size;
			Program.Preferences.ThumbnailSizeIsInPixels = _thumbnailPixelsRadio.Checked;
			int thumbnailPercentage = int.Parse(_thumbnailPercentageTextBox.Text);
			Program.Preferences.ThumbnailPercentage = thumbnailPercentage;
		}

		private void InitializeComponent() {
			this.CreateGeneralPage();
			this.CreateServerTreePage();
			this.CreateClientAreaPage();
			this.CreateHotKeysPage();
			this.CreateExperiencePage();
			this.CreateFullScreenPage();
			this.InitButtons();
			this.ScaleAndLayout();
		}

		private TabPage NewTabPage(string name) {
			TabPage tabPage = new SettingsTabPage {
				Location = FormTools.TopLeftLocation(),
				Size = new Size(FormTools.TabControlWidth-8, FormTools.TabControlHeight-14),
				Text = name,
				AutoSize = true
			};
			AddTabPage(tabPage);
			return tabPage;
		}

		private TabPage CreateFullScreenPage() {
			int rowIndex = 0;
			int num = 0;
			TabPage tabPage = NewTabPage("全屏");
			_connectionBarEnabledCheckBox = FormTools.NewCheckBox("显示全屏连接栏", 0, rowIndex++, num++);
			_connectionBarEnabledCheckBox.CheckedChanged += ConnectionBarEnabledCheckedChanged;
			_connectionBarAutoHiddenCheckBox = FormTools.NewCheckBox("自动隐藏连接栏", 0, rowIndex++, num++);
			_connectionBarAutoHiddenCheckBox.Location = new Point(_connectionBarEnabledCheckBox.Left + 24, _connectionBarAutoHiddenCheckBox.Top);
			FormTools.AddCheckBox(tabPage, "全屏窗口始终位于最顶层", Program.Preferences.Settings.FullScreenWindowIsTopMost, 0, ref rowIndex, ref num);
			if (RdpClient.SupportsMonitorSpanning) {
				FormTools.AddCheckBox(tabPage, "必要时使用多台显示器", Program.Preferences.Settings.UseMultipleMonitors, 0, ref rowIndex, ref num);
			}
			if (RdpClient.SupportsPanning) {
				_enablePanningCheckBox = FormTools.NewCheckBox("使用滑动而不是滚动条", 0, rowIndex++, num++);
				_enablePanningCheckBox.Setting = Program.Preferences.Settings.EnablePanning;
				_enablePanningCheckBox.CheckedChanged += EnablePanningCheckedChanged;
				Label label = FormTools.NewLabel("滑动速度", 0, rowIndex);
				label.Size = new Size(116, FormTools.ControlHeight);
				label.Location = new Point(_enablePanningCheckBox.Left + 24, label.Top);
				_panningAccelerationUpDown = new RdcNumericUpDown {
					Location = FormTools.NewLocation(1, rowIndex++),
					Minimum = 1m,
					Maximum = 9m,
					Size = new Size(40, FormTools.ControlHeight),
					TabIndex = num++,
					Setting = Program.Preferences.Settings.PanningAcceleration
				};
				tabPage.Controls.Add(_enablePanningCheckBox, label, _panningAccelerationUpDown);
			}
			tabPage.Controls.Add(_connectionBarEnabledCheckBox, _connectionBarAutoHiddenCheckBox);
			return tabPage;
		}

		private TabPage CreateExperiencePage() {
			TabPage tabPage = NewTabPage("体验");
			int rowIndex = 0;
			int num = 0;
			_bandwidthComboBox = FormTools.AddLabeledValueDropDown(tabPage, "连接速度(&S)", ref rowIndex, ref num, (BandwidthItem v) => v.Text, _bandwidthItems);
			_bandwidthComboBox.SelectedIndexChanged += BandwidthCombo_ControlChanged;
			Label label = FormTools.NewLabel("允许以下:", 0, rowIndex);
			_desktopBackgroundCheckBox = FormTools.NewCheckBox("桌面背景", 1, rowIndex++, num++);
			_desktopBackgroundCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_fontSmoothingCheckBox = FormTools.NewCheckBox("字体平滑", 1, rowIndex++, num++);
			_fontSmoothingCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_desktopCompositionCheckBox = FormTools.NewCheckBox("桌面拼合", 1, rowIndex++, num++);
			_desktopCompositionCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_windowDragCheckBox = FormTools.NewCheckBox("拖动时显示窗口内容", 1, rowIndex++, num++);
			_windowDragCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_menuAnimationCheckBox = FormTools.NewCheckBox("菜单和窗口动画", 1, rowIndex++, num++);
			_menuAnimationCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			_themesCheckBox = FormTools.NewCheckBox("主题", 1, rowIndex++, num++);
			_themesCheckBox.CheckedChanged += PerfCheckBox_CheckedChanged;
			rowIndex++;
			_persistentBitmapCachingCheckBox = FormTools.NewCheckBox("持久位图缓存", 1, rowIndex++, num++);
			tabPage.Controls.Add(label, _desktopBackgroundCheckBox, _fontSmoothingCheckBox, _desktopCompositionCheckBox, _windowDragCheckBox, _menuAnimationCheckBox, _themesCheckBox, _persistentBitmapCachingCheckBox);
			return tabPage;
		}

		private TabPage CreateHotKeysPage() {
			GlobalSettings settings = Program.Preferences.Settings;
			TabPage tabPage = NewTabPage("热键");
			GroupBox groupBox = new GroupBox {
				Text = "ALT 热键（仅在 Windows 组合键未重定向时有效）"
			};
			int rowIndex = 0;
			int num = 0;
			AddHotKeyBox(groupBox, "ALT+TAB", "ALT+", settings.HotKeyAltTab, ref rowIndex, ref num);
			AddHotKeyBox(groupBox, "ALT+SHIFT+TAB", "ALT+", settings.HotKeyAltShiftTab, ref rowIndex, ref num);
			AddHotKeyBox(groupBox, "ALT+ESC", "ALT+", settings.HotKeyAltEsc, ref rowIndex, ref num);
			AddHotKeyBox(groupBox, "ALT+SPACE", "ALT+", settings.HotKeyAltSpace, ref rowIndex, ref num);
			AddHotKeyBox(groupBox, "CTRL+ESC", "ALT+", settings.HotKeyCtrlEsc, ref rowIndex, ref num);
			groupBox.SizeAndLocate(null);
			GroupBox groupBox2 = new GroupBox {
				Text = "CTRL+ALT 热键（始终有效）"
			};
			rowIndex = 0;
			num = 0;
			AddHotKeyBox(groupBox2, "CTRL+ALT+DEL", "CTRL+ALT+", settings.HotKeyCtrlAltDel, ref rowIndex, ref num);
			AddHotKeyBox(groupBox2, "全屏", "CTRL+ALT+", settings.HotKeyFullScreen, ref rowIndex, ref num);
			AddHotKeyBox(groupBox2, "上一个会话", "CTRL+ALT+", settings.HotKeyFocusReleaseLeft, ref rowIndex, ref num);
			AddHotKeyBox(groupBox2, "选择会话", "CTRL+ALT+", settings.HotKeyFocusReleaseRight, ref rowIndex, ref num);
			groupBox2.SizeAndLocate(groupBox);
			tabPage.Controls.Add(groupBox, groupBox2);
			return tabPage;
		}

		private void AddHotKeyBox(Control parent, string label, string prefix, EnumSetting<Keys> setting, ref int rowIndex, ref int tabIndex) {
			parent.Controls.Add(FormTools.NewLabel(label, 0, rowIndex));
			HotKeyBox value = new HotKeyBox {
				Prefix = prefix,
				Location = FormTools.NewLocation(1, rowIndex++),
				Size = new Size(340, FormTools.ControlHeight),
				TabIndex = tabIndex++,
				Setting = setting
			};
			parent.Controls.Add(value);
		}

		private TabPage CreateClientAreaPage() {
			TabPage tabPage = NewTabPage("客户区");
			_casSizeGroup = new GroupBox {
				Text = "客户区大小"
			};
			RdcCheckBox value = new RdcCheckBox {
				Size = new Size(480, 24),
				Text = "锁定窗口大小(&L)",
				Location = FormTools.NewLocation(0, 0),
				TabIndex = 0,
				TabStop = true,
				Setting = Program.Preferences.Settings.LockWindowSize
			};
			_casSizeGroup.Controls.Add(value);
			_casSizeGroup.Controls.AddRange(FormTools.NewSizeRadios());
			_casCustomRadio = new RadioButton {
				Size = new Size(86, 24),
				Text = "自定义(&C)"
			};
			_casSizeGroup.Controls.Add(_casCustomRadio);
			_casCustomButton = new Button {
				Location = new Point(_casCustomRadio.Right + 10, _casCustomRadio.Location.Y),
				TabIndex = _casCustomRadio.TabIndex + 1
			};
			_casCustomButton.Click += CustomSizeClick;
			_casSizeGroup.Controls.Add(_casCustomButton);
			FormTools.LayoutGroupBox(_casSizeGroup, 2, null, 0, 0);

			GroupBox groupBox = new GroupBox {
				Size = new Size(512, 72),
				Text = "缩略图单位大小"
			};
			_thumbnailPixelsRadio = new RadioButton {
				Size = new Size(80, 24),
				Text = "像素(&X)"
			};
			_thumbnailPercentageRadio = new RadioButton {
				Size = new Size(88, 24),
				Text = "百分比(&R)"
			};
			_thumbnailPercentageRadio.CheckedChanged += ThumbnailPercentageRadioCheckedChanged;
			groupBox.Controls.Add(_thumbnailPixelsRadio, _thumbnailPercentageRadio);

			int num = Math.Max(_thumbnailPixelsRadio.Right, _thumbnailPercentageRadio.Right);
			_thumbnailPixelsButton = new Button {
				Location = new Point(num + 10, _thumbnailPixelsRadio.Location.Y),
				TabIndex = _thumbnailPercentageRadio.TabIndex + 1
			};
			_thumbnailPixelsButton.Click += CustomSizeClick;
			_thumbnailPercentageTextBox = new NumericTextBox(1, 100, "百分比必须在 1 到 100 之间（含）") {
				Enabled = false,
				Location = new Point(num + 11, _thumbnailPercentageRadio.Location.Y + 2),
				Size = new Size(72, FormTools.ControlHeight),
				TabIndex = _thumbnailPercentageRadio.TabIndex + 1
			};
			groupBox.Controls.Add(_thumbnailPixelsButton, _thumbnailPercentageTextBox);
			FormTools.LayoutGroupBox(groupBox, 2, _casSizeGroup);
			tabPage.Controls.Add(_casSizeGroup, groupBox);
			return tabPage;
		}

		private TabPage CreateServerTreePage() {
			int rowIndex = 0;
			int num = 0;
			TabPage tabPage = NewTabPage("树");
			GroupBox groupBox = new GroupBox {
				Text = "服务器树"
			};
			FormTools.AddCheckBox(groupBox, "单击以选择将焦点转移到远程客户端", Program.Preferences.Settings.FocusOnClick, 0, ref rowIndex, ref num);
			FormTools.AddCheckBox(groupBox, "树控件处于非活动状态时使节点变暗", Program.Preferences.Settings.DimNodesWhenInactive, 0, ref rowIndex, ref num);
			_treeLocationCombo = FormTools.AddLabeledValueDropDown(groupBox, "位置", ref rowIndex, ref num, (DockStyle v) => v.ToString(), new DockStyle[2]
			{
				DockStyle.Left,
				DockStyle.Right
			});
			_treeVisibilityCombo = FormTools.AddLabeledValueDropDown(groupBox, "可见性", ref rowIndex, ref num, (ControlVisibility v) => v.ToString(), new ControlVisibility[3]
			{
				ControlVisibility.Dock,
				ControlVisibility.AutoHide,
				ControlVisibility.Hide
			});
			Label label = FormTools.NewLabel("弹出延迟：", 0, rowIndex++);
			label.Left += 66;
			label.Size = new Size(80, label.Height);
			NumericTextBox serverTreeAutoHidePopUpDelay = new NumericTextBox(0, 1000, "自动隐藏弹出延迟必须为 0 到 1000 毫秒") {
				Enabled = false,
				Location = new Point(label.Right, label.Top),
				Size = new Size(40, 24),
				Setting = Program.Preferences.Settings.ServerTreeAutoHidePopUpDelay,
				TabStop = true,
				TabIndex = num++
			};
			_treeVisibilityCombo.SelectedIndexChanged += delegate {
				serverTreeAutoHidePopUpDelay.Enabled = _treeVisibilityCombo.SelectedValue == ControlVisibility.AutoHide;
			};
			groupBox.AddControlsAndSizeGroup(label);
			Label label2 = new Label {
				Location = new Point(serverTreeAutoHidePopUpDelay.Right + 3, label.Top+4),
				Size = new Size(80, 24),
				Text = "毫秒"
			};
			groupBox.Controls.Add(serverTreeAutoHidePopUpDelay, label2);
			groupBox.SizeAndLocate(null);
			groupBox.Height -= 6;
			_virtualGroupsGroup = new GroupBox {
				Text = "虚拟组"
			};
			foreach (IBuiltInVirtualGroup item in Program.BuiltInVirtualGroups.Where((IBuiltInVirtualGroup group) => group.IsVisibilityConfigurable)) {
				_virtualGroupsGroup.Controls.Add(new CheckBox {
					Size = new Size(112, 24),
					Tag = item,
					Text = item.Text
				});
			}
			FormTools.LayoutGroupBox(_virtualGroupsGroup, 2, groupBox);
			_virtualGroupsGroup.Height -= 6;
			rowIndex = 0;
			num = 0;
			GroupBox groupBox2 = new GroupBox {
				Text = "排序"
			};
			FormTools.AddLabeledValueDropDown(groupBox2, "组排序顺序", Program.Preferences.Settings.GroupSortOrder, ref rowIndex, ref num, Helpers.SortOrderToString, new SortOrder[2] {
				SortOrder.ByName,
				SortOrder.None
			});
			FormTools.AddLabeledEnumDropDown(groupBox2, "服务器排序顺序", Program.Preferences.Settings.ServerSortOrder, ref rowIndex, ref num, Helpers.SortOrderToString);
			FormTools.LayoutGroupBox(groupBox2, 2, _virtualGroupsGroup);
			tabPage.Controls.Add(groupBox, _virtualGroupsGroup, groupBox2);
			return tabPage;
		}

		private TabPage CreateGeneralPage() {
			int rowIndex = 0;
			int num = 0;
			TabPage tabPage = NewTabPage("常规");
			FormTools.AddCheckBox(tabPage, "隐藏主菜单，直到按下 ALT", Program.Preferences.Settings.HideMainMenu, 0, ref rowIndex, ref num);
			RdcCheckBox autoSaveCheckBox = FormTools.AddCheckBox(tabPage, "自动保存间隔:", Program.Preferences.Settings.AutoSaveFiles, 0, ref rowIndex, ref num);
			autoSaveCheckBox.Size = new Size(120, 24);
			NumericTextBox autoSaveInterval = new NumericTextBox(0, 60, "自动保存间隔必须为 0 到 60 分钟（含）") {
				Location = new Point(autoSaveCheckBox.Right + 1, autoSaveCheckBox.Top + 2),
				Size = new Size(FormTools.ControlHeight, 24),
				TabIndex = num++,
				TabStop = true,
				Enabled = false
			};
			autoSaveInterval.Setting = Program.Preferences.Settings.AutoSaveInterval;
			autoSaveCheckBox.CheckedChanged += delegate {
				autoSaveInterval.Enabled = autoSaveCheckBox.Checked;
			};
			Label label = new Label {
				Location = new Point(autoSaveInterval.Right + 3, autoSaveCheckBox.Top + 4),
				Size = new Size(60, 24),
				Text = "分钟"
			};
			RdcCheckBox rdcCheckBox = FormTools.AddCheckBox(tabPage, "启动时提示重新连接已连接的服务器", Program.Preferences.Settings.ReconnectOnStartup, 0, ref rowIndex, ref num);
			Button button = new Button {
				Location = new Point(8, rdcCheckBox.Bottom + 8),
				TabIndex = num++,
				Text = "默认组设置...",
				Width = 140
			};
			button.Click += delegate {
				DefaultSettingsGroup.Instance.DoPropertiesDialog();
			};
			tabPage.Controls.Add(autoSaveCheckBox, autoSaveInterval, label, button);
			return tabPage;
		}

		private void EnablePanningCheckedChanged(object sender, EventArgs e) {
			_panningAccelerationUpDown.Enabled = _enablePanningCheckBox.Checked;
		}

		private void ConnectionBarEnabledCheckedChanged(object sender, EventArgs e) {
			_connectionBarAutoHiddenCheckBox.Enabled = _connectionBarEnabledCheckBox.Checked;
		}

		private void CustomSizeClick(object sender, EventArgs e) {
			Button button = sender as Button;
			Size size = SizeHelper.Parse(button.Text);
			using CustomSizeDialog customSizeDialog = new CustomSizeDialog(size);
			if (customSizeDialog.ShowDialog() == DialogResult.OK) {
				button.Text = customSizeDialog.WidthText + SizeHelper.Separator + customSizeDialog.HeightText;
				_thumbnailPixelsRadio.Checked = true;
			}
		}

		private void ThumbnailPercentageRadioCheckedChanged(object sender, EventArgs e) {
			_thumbnailPercentageTextBox.Enabled = (sender as RadioButton).Checked;
			_thumbnailPixelsButton.Enabled = !_thumbnailPercentageTextBox.Enabled;
		}

		private void BandwidthCombo_ControlChanged(object sender, EventArgs e) {
			BandwidthSettingsChanged();
		}

		private void BandwidthSettingsChanged() {
			if (!_inHandler) {
				_inHandler = true;
				SetBandwidthCheckBoxes(_bandwidthComboBox.SelectedValue.Flags);
				_inHandler = false;
			}
		}

		private void SetBandwidthCheckBoxes(int flags) {
			_desktopBackgroundCheckBox.Checked = (flags & 1) == 0;
			_fontSmoothingCheckBox.Checked = (flags & 0x80) != 0;
			_desktopCompositionCheckBox.Checked = (flags & 0x100) != 0;
			_windowDragCheckBox.Checked = (flags & 2) == 0;
			_menuAnimationCheckBox.Checked = (flags & 4) == 0;
			_themesCheckBox.Checked = (flags & 8) == 0;
		}

		private void PerfCheckBox_CheckedChanged(object sender, EventArgs e) {
			if (!_inHandler) {
				_inHandler = true;
				int flags = ComputeFlagsFromCheckBoxes();
				BandwidthItem selectedValue = _bandwidthItems.Where((BandwidthItem i) => i.Flags == flags).FirstOrDefault() ?? _bandwidthItems.First((BandwidthItem i) => i.Text.Equals("自定义"));
				_bandwidthComboBox.SelectedValue = selectedValue;
				_inHandler = false;
			}
		}

		private int ComputeFlagsFromCheckBoxes() {
			int num = 0;
			if (!_desktopBackgroundCheckBox.Checked)
				num |= 1;
			if (_fontSmoothingCheckBox.Checked)
				num |= 0x80;
			if (_desktopCompositionCheckBox.Checked)
				num |= 0x100;
			if (!_windowDragCheckBox.Checked)
				num |= 2;
			if (!_menuAnimationCheckBox.Checked)
				num |= 4;
			if (!_themesCheckBox.Checked)
				num |= 8;

			return num;
		}
	}
}

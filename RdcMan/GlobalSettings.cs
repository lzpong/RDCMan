using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan {
	public sealed class GlobalSettings : SettingsGroup {
		//public const string TabName = "Preferences";

		private static Dictionary<string, SettingProperty> _settingProperties;

		protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

		[Setting("AutoSaveFiles")]
		public BoolSetting AutoSaveFiles { get; private set; }

		[Setting("AutoSaveInterval", DefaultValue = 1)]
		public IntSetting AutoSaveInterval { get; private set; }

		[Setting("BuiltInGroups")]
		public XmlSetting BuiltInGroups { get; private set; }

		[Setting("ConnectionBarState", DefaultValue = RdpClient.ConnectionBarState.Pinned)]
		public EnumSetting<RdpClient.ConnectionBarState> ConnectionBarState { get; private set; }

		[Setting("CredentialsProfiles")]
		public XmlSetting CredentialsProfiles { get; private set; }

		[Setting("DefaultGroupSettings")]
		public XmlSetting DefaultGroupSettings { get; private set; }

		[Setting("DimNodesWhenInactive")]
		public BoolSetting DimNodesWhenInactive { get; private set; }

		[Setting("EnablePanning")]
		public BoolSetting EnablePanning { get; private set; }

		[Setting("FilesToOpen")]
		public ListSetting<string> FilesToOpen { get; private set; }

		[Setting("FocusOnClick")]
		public BoolSetting FocusOnClick { get; private set; }

		[Setting("FullScreenWindowIsTopMost")]
		public BoolSetting FullScreenWindowIsTopMost { get; private set; }

		[Setting("GroupSortOrder", DefaultValue = SortOrder.ByName)]
		public EnumSetting<SortOrder> GroupSortOrder { get; private set; }

		[Setting("HideMainMenu")]
		public BoolSetting HideMainMenu { get; private set; }

		[Setting("HotKeyAltEsc", DefaultValue = Keys.Insert)]
		public EnumSetting<Keys> HotKeyAltEsc { get; private set; }

		[Setting("HotKeyAltShiftTab", DefaultValue = Keys.Next)]
		public EnumSetting<Keys> HotKeyAltShiftTab { get; private set; }

		[Setting("HotKeyAltSpace", DefaultValue = Keys.Delete)]
		public EnumSetting<Keys> HotKeyAltSpace { get; private set; }

		[Setting("HotKeyAltTab", DefaultValue = Keys.Prior)]
		public EnumSetting<Keys> HotKeyAltTab { get; private set; }

		[Setting("HotKeyCtrlAltDel", DefaultValue = Keys.End)]
		public EnumSetting<Keys> HotKeyCtrlAltDel { get; private set; }

		[Setting("HotKeyCtrlEsc", DefaultValue = Keys.Home)]
		public EnumSetting<Keys> HotKeyCtrlEsc { get; private set; }

		[Setting("HotKeyFocusReleaseLeft", DefaultValue = Keys.Left)]
		public EnumSetting<Keys> HotKeyFocusReleaseLeft { get; private set; }

		[Setting("HotKeyFocusReleaseRight", DefaultValue = Keys.Right)]
		public EnumSetting<Keys> HotKeyFocusReleaseRight { get; private set; }

		[Setting("HotKeyFullScreen", DefaultValue = Keys.Cancel)]
		public EnumSetting<Keys> HotKeyFullScreen { get; private set; }

		[Setting("LastUpdateCheckTimeUtc", DefaultValue = "2012-06-01 00:00:00Z")]
		public StringSetting LastUpdateCheckTimeUtc { get; private set; }

		[Setting("LockWindowSize")]
		public BoolSetting LockWindowSize { get; private set; }

		[Setting("PanningAcceleration", DefaultValue = 1)]
		public IntSetting PanningAcceleration { get; private set; }

		[Setting("PerformanceFlags")]
		public IntSetting PerformanceFlags { get; private set; }

		[Setting("PersistentBitmapCaching")]
		public BoolSetting PersistentBitmapCaching { get; private set; }

		[Setting("PluginSettings")]
		public XmlSetting PluginSettings { get; private set; }

		[Setting("ReconnectOnStartup", DefaultValue = true)]
		public BoolSetting ReconnectOnStartup { get; private set; }

		[Setting("ServerSortOrder", DefaultValue = SortOrder.ByStatus)]
		public EnumSetting<SortOrder> ServerSortOrder { get; private set; }

		[Setting("ServerTreeAutoHidePopUpDelay")]
		public IntSetting ServerTreeAutoHidePopUpDelay { get; private set; }

		[Setting("ServerTreeLocation", DefaultValue = DockStyle.Left)]
		public EnumSetting<DockStyle> ServerTreeLocation { get; private set; }

		[Setting("ServerTreeVisibility", DefaultValue = ControlVisibility.Dock)]
		public EnumSetting<ControlVisibility> ServerTreeVisibility { get; private set; }

		[Setting("ServerTreeWidth", DefaultValue = 200)]
		public IntSetting ServerTreeWidth { get; private set; }

		[Setting("ShowConnectedGroup")]
		public BoolSetting ShowConnectedGroup { get; private set; }

		/// <summary>
		/// lzpong 内置组
		/// </summary>
		[Setting("ShowInternalGroup")]
		public BoolSetting ShowBuiltInGroup { get; private set; }

		[Setting("ShowFavoritesGroup")]
		public BoolSetting ShowFavoritesGroup { get; private set; }

		[Setting("ShowRecentlyUsedGroup")]
		public BoolSetting ShowRecentlyUsedGroup { get; private set; }

		[Setting("ShowReconnectGroup")]
		public BoolSetting ShowReconnectGroup { get; private set; }

		[Setting("SmartSizeUndockedWindows", IsObsolete = true)]
		public BoolSetting SmartSizeUndockedWindows { get; private set; }

		[Setting("ThumbnailPercentage", DefaultValue = 15)]
		public IntSetting ThumbnailPercentage { get; private set; }

		[Setting("ThumbnailSize")]
		public SizeSetting ThumbnailSize { get; private set; }

		[Setting("ThumbnailSizeIsInPixels", DefaultValue = true)]
		public BoolSetting ThumbnailSizeIsInPixels { get; private set; }

		[Setting("UseMultipleMonitors")]
		public BoolSetting UseMultipleMonitors { get; private set; }

		[Setting("WindowIsMaximized")]
		public BoolSetting WindowIsMaximized { get; private set; }

		[Setting("WindowPosition")]
		public PointSetting WindowPosition { get; private set; }

		[Setting("WindowSize")]
		public SizeSetting WindowSize { get; private set; }

		static GlobalSettings() {
			typeof(GlobalSettings).GetSettingProperties(out _settingProperties);
			_settingProperties["ThumbnailSize"].Attribute.DefaultValue = new Size(160, 120);
			_settingProperties["WindowPosition"].Attribute.DefaultValue = new Point(200, 200);
			_settingProperties["WindowSize"].Attribute.DefaultValue = new Size(1273, 823);
		}

		public GlobalSettings()
			: base("首选项", "Settings") {
			base.InheritSettingsType.Mode = InheritanceMode.Disabled;
		}

		public void TransferPreferences(Preferences prefs) {
			foreach (string key in _settingProperties.Keys) {
				try {
					object transferValue = prefs.GetTransferValue(key);
					SetValue(key, transferValue);
				}
				catch { }
			}
		}

		public object GetValue(string name) {
			object value = _settingProperties[name].Property.GetValue(this, null);
			return value.GetType().GetProperty("Value").GetValue(value, null);
		}

		public void SetValue(string name, object value) {
			object value2 = _settingProperties[name].Property.GetValue(this, null);
			value2.GetType().GetProperty("Value").SetValue(value2, value, null);
		}

		public override TabPage CreateTabPage(TabbedSettingsDialog dialog) {
			throw new NotImplementedException();
		}

		protected override void WriteSettings(XmlTextWriter tw, RdcTreeNode node) {
			tw.WriteAttributeString("programVersion", Program.TheForm.VersionText);
			base.WriteSettings(tw, node);
		}

		protected override void Copy(RdcTreeNode node) {
			throw new InvalidOperationException("GlobalSettings should never be copied");
		}
	}
}

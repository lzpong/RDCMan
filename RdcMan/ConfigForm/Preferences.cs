using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	[SettingsProvider(typeof(RdcSettingsProvider))]
	public sealed class Preferences : ApplicationSettingsBase
	{
		public bool NeedToSave
		{
			get;
			set;
		}

		public GlobalSettings Settings { get; private set; }

		public override object this[string propertyName]
		{
			get
			{
				return Settings.GetValue(propertyName);
			}
			set
			{
				Settings.SetValue(propertyName, value);
			}
		}

		private string SettingsDirectory { get; set; }

		private string SettingsPath { get; set; }

		[DefaultSettingValue("False")]
		[UserScopedSetting]
		public bool AutoSaveFiles
		{
			get
			{
				return (bool)this["AutoSaveFiles"];
			}
			set
			{
				this["AutoSaveFiles"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("0")]
		public int AutoSaveInterval
		{
			get
			{
				return (int)this["AutoSaveInterval"];
			}
			set
			{
				this["AutoSaveInterval"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("False")]
		public bool ShowConnectedGroup
		{
			get
			{
				return (bool)this["ShowConnectedGroup"];
			}
			set
			{
				this["ShowConnectedGroup"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("False")]
		public bool ShowFavoritesGroup
		{
			get
			{
				return (bool)this["ShowFavoritesGroup"];
			}
			set
			{
				this["ShowFavoritesGroup"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("False")]
		public bool ShowReconnectGroup
		{
			get
			{
				return (bool)this["ShowReconnectGroup"];
			}
			set
			{
				this["ShowReconnectGroup"] = value;
			}
		}

		[DefaultSettingValue("False")]
		[UserScopedSetting]
		public bool ShowRecentlyUsedGroup
		{
			get
			{
				return (bool)this["ShowRecentlyUsedGroup"];
			}
			set
			{
				this["ShowRecentlyUsedGroup"] = value;
			}
		}

		[UserScopedSetting]
		public List<string> FilesToOpen
		{
			get
			{
				return (List<string>)this["FilesToOpen"];
			}
			set
			{
				this["FilesToOpen"] = value;
			}
		}

		[UserScopedSetting]
		public byte[] CredentialsProfiles
		{
			get
			{
				return (byte[])base["CredentialsProfiles"];
			}
			set
			{
				base["CredentialsProfiles"] = value;
			}
		}

		[UserScopedSetting]
		public XmlDocument CredentialsProfilesXml
		{
			get
			{
				return (XmlDocument)this["CredentialsProfilesXml"];
			}
			set
			{
				this["CredentialsProfilesXml"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("False")]
		public bool FocusOnClick
		{
			get
			{
				return (bool)this["FocusOnClick"];
			}
			set
			{
				this["FocusOnClick"] = value;
			}
		}

		[DefaultSettingValue("False")]
		[UserScopedSetting]
		public bool DimNodesWhenInactive
		{
			get
			{
				return (bool)this["DimNodesWhenInactive"];
			}
			set
			{
				this["DimNodesWhenInactive"] = value;
			}
		}

		[DefaultSettingValue("False")]
		[UserScopedSetting]
		public bool FullScreenWindowIsTopMost
		{
			get
			{
				return (bool)this["FullScreenWindowIsTopMost"];
			}
			set
			{
				this["FullScreenWindowIsTopMost"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("False")]
		public bool UseMultipleMonitors
		{
			get
			{
				return (bool)this["UseMultipleMonitors"];
			}
			set
			{
				this["UseMultipleMonitors"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("False")]
		public bool EnablePanning
		{
			get
			{
				return (bool)this["EnablePanning"];
			}
			set
			{
				this["EnablePanning"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("1")]
		public int PanningAcceleration
		{
			get
			{
				return (int)this["PanningAcceleration"];
			}
			set
			{
				this["PanningAcceleration"] = value;
			}
		}

		[DefaultSettingValue("ByName")]
		[UserScopedSetting]
		public SortOrder GroupSortOrder
		{
			get
			{
				return (SortOrder)this["GroupSortOrder"];
			}
			set
			{
				this["GroupSortOrder"] = value;
			}
		}

		[DefaultSettingValue("ByStatus")]
		[UserScopedSetting]
		public SortOrder ServerSortOrder
		{
			get
			{
				return (SortOrder)this["ServerSortOrder"];
			}
			set
			{
				this["ServerSortOrder"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("Dock")]
		public ControlVisibility ServerTreeVisibility
		{
			get
			{
				return (ControlVisibility)this["ServerTreeVisibility"];
			}
			set
			{
				this["ServerTreeVisibility"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("0")]
		public int ServerTreeAutoHidePopUpDelay
		{
			get
			{
				return (int)this["ServerTreeAutoHidePopUpDelay"];
			}
			set
			{
				this["ServerTreeAutoHidePopUpDelay"] = value;
			}
		}

		[DefaultSettingValue("Left")]
		[UserScopedSetting]
		public DockStyle ServerTreeLocation
		{
			get
			{
				return (DockStyle)this["ServerTreeLocation"];
			}
			set
			{
				this["ServerTreeLocation"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("True")]
		public bool ThumbnailSizeIsInPixels
		{
			get
			{
				return (bool)this["ThumbnailSizeIsInPixels"];
			}
			set
			{
				this["ThumbnailSizeIsInPixels"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("160, 120")]
		public Size ThumbnailSize
		{
			get
			{
				return (Size)this["ThumbnailSize"];
			}
			set
			{
				this["ThumbnailSize"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("15")]
		public int ThumbnailPercentage
		{
			get
			{
				return (int)this["ThumbnailPercentage"];
			}
			set
			{
				this["ThumbnailPercentage"] = value;
			}
		}

		[UserScopedSetting]
		public byte[] DefaultGroupSettings
		{
			get
			{
				return (byte[])base["DefaultGroupSettings"];
			}
			set
			{
				base["DefaultGroupSettings"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("0")]
		public int PerformanceFlags
		{
			get
			{
				return (int)this["PerformanceFlags"];
			}
			set
			{
				this["PerformanceFlags"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("False")]
		public bool WindowIsMaximized
		{
			get
			{
				return (bool)this["WindowIsMaximized"];
			}
			set
			{
				this["WindowIsMaximized"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("200, 200")]
		public Point WindowPosition
		{
			get
			{
				return (Point)this["WindowPosition"];
			}
			set
			{
				this["WindowPosition"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("200")]
		public int ServerTreeWidth
		{
			get
			{
				return (int)this["ServerTreeWidth"];
			}
			set
			{
				this["ServerTreeWidth"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("Pinned")]
		public RdpClient.ConnectionBarState ConnectionBarState
		{
			get
			{
				return (RdpClient.ConnectionBarState)this["ConnectionBarState"];
			}
			set
			{
				this["ConnectionBarState"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("1273, 823")]
		public Size WindowSize
		{
			get
			{
				return (Size)this["WindowSize"];
			}
			set
			{
				this["WindowSize"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("False")]
		public bool LockWindowSize
		{
			get
			{
				return (bool)this["LockWindowSize"];
			}
			set
			{
				this["LockWindowSize"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("Insert")]
		public Keys HotKeyAltEsc
		{
			get
			{
				return (Keys)this["HotKeyAltEsc"];
			}
			set
			{
				this["HotKeyAltEsc"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("Home")]
		public Keys HotKeyCtrlEsc
		{
			get
			{
				return (Keys)this["HotKeyCtrlEsc"];
			}
			set
			{
				this["HotKeyCtrlEsc"] = value;
			}
		}

		[DefaultSettingValue("PageUp")]
		[UserScopedSetting]
		public Keys HotKeyAltTab
		{
			get
			{
				return (Keys)this["HotKeyAltTab"];
			}
			set
			{
				this["HotKeyAltTab"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("PageDown")]
		public Keys HotKeyAltShiftTab
		{
			get
			{
				return (Keys)this["HotKeyAltShiftTab"];
			}
			set
			{
				this["HotKeyAltShiftTab"] = value;
			}
		}

		[DefaultSettingValue("Delete")]
		[UserScopedSetting]
		public Keys HotKeyAltSpace
		{
			get
			{
				return (Keys)this["HotKeyAltSpace"];
			}
			set
			{
				this["HotKeyAltSpace"] = value;
			}
		}

		[DefaultSettingValue("End")]
		[UserScopedSetting]
		public Keys HotKeyCtrlAltDel
		{
			get
			{
				return (Keys)this["HotKeyCtrlAltDel"];
			}
			set
			{
				this["HotKeyCtrlAltDel"] = value;
			}
		}

		[DefaultSettingValue("Cancel")]
		[UserScopedSetting]
		public Keys HotKeyFullScreen
		{
			get
			{
				return (Keys)this["HotKeyFullScreen"];
			}
			set
			{
				this["HotKeyFullScreen"] = value;
			}
		}

		[DefaultSettingValue("Left")]
		[UserScopedSetting]
		public Keys HotKeyFocusReleaseLeft
		{
			get
			{
				return (Keys)this["HotKeyFocusReleaseLeft"];
			}
			set
			{
				this["HotKeyFocusReleaseLeft"] = value;
			}
		}

		[DefaultSettingValue("Right")]
		[UserScopedSetting]
		public Keys HotKeyFocusReleaseRight
		{
			get
			{
				return (Keys)this["HotKeyFocusReleaseRight"];
			}
			set
			{
				this["HotKeyFocusReleaseRight"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("2012-06-01 00:00:00Z")]
		public string LastUpdateCheckTimeUtc
		{
			get
			{
				return (string)this["LastUpdateCheckTimeUtc"];
			}
			set
			{
				this["LastUpdateCheckTimeUtc"] = value;
			}
		}

		[UserScopedSetting]
		[DefaultSettingValue("False")]
		public bool HideMainMenu
		{
			get
			{
				return (bool)this["HideMainMenu"];
			}
			set
			{
				this["HideMainMenu"] = value;
			}
		}

		[DefaultSettingValue("True")]
		[UserScopedSetting]
		public bool ReconnectOnStartup
		{
			get
			{
				return (bool)this["ReconnectOnStartup"];
			}
			set
			{
				this["ReconnectOnStartup"] = value;
			}
		}

		private Preferences()
		{
			Settings = new GlobalSettings();
			string name = Assembly.GetExecutingAssembly().GetName().Name;
			SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.CompanyName, Application.ProductName);
			SettingsPath = Path.Combine(SettingsDirectory, name + ".settings");
		}

		public object GetTransferValue(string name)
		{
			return base[name];
		}

		public static Preferences Load()
		{
			Preferences preferences = new Preferences();
			if (Program.ResetPreferences)
			{
				return preferences;
			}
			List<string> list = new List<string>();
			//将旧版配置文件移动到新版位置
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft Corporation", Application.ProductName);
			string text2 = Path.Combine(text, "RDCMan.settings");
			if (File.Exists(text2))
			{
				if (!File.Exists(preferences.SettingsPath))
				{
					Directory.CreateDirectory(preferences.SettingsDirectory);
					File.Move(text2, preferences.SettingsPath);
				}
				try
				{
					Directory.Delete(text);
				}
				catch
				{
				}
			}
			bool flag = true;
			try
			{
				using (XmlTextReader reader = new XmlTextReader(preferences.SettingsPath))
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(reader);
					XmlNode lastChild = xmlDocument.LastChild;
					try
					{
						_ = lastChild.Attributes["programVersion"].Value;
						preferences.Settings.ReadXml(lastChild, null, list);
						flag = false;
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			if (flag)
			{
				preferences.Settings.TransferPreferences(preferences);
				if (preferences.DefaultGroupSettings != null)
				{
					MemoryStream input = new MemoryStream(preferences.DefaultGroupSettings);
					XmlTextReader xmlTextReader = new XmlTextReader(input);
					xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
					XmlTextReader xmlTextReader2 = xmlTextReader;
					xmlTextReader2.MoveToContent();
					XmlDocument xmlDocument2 = new XmlDocument();
					XmlNode xmlNode = xmlDocument2.ReadNode(xmlTextReader2);
					xmlTextReader2.Close();
					GroupBase.SchemaVersion = 2;
					DefaultSettingsGroup.Instance.ReadXml(xmlNode, list);
				}
				if (preferences.CredentialsProfiles != null)
				{
					MemoryStream input2 = new MemoryStream(preferences.CredentialsProfiles);
					XmlTextReader xmlTextReader3 = new XmlTextReader(input2);
					xmlTextReader3.WhitespaceHandling = WhitespaceHandling.None;
					XmlTextReader xmlTextReader4 = xmlTextReader3;
					xmlTextReader4.MoveToContent();
					XmlDocument xmlDocument3 = new XmlDocument();
					XmlNode xmlNode2 = xmlDocument3.ReadNode(xmlTextReader4);
					xmlTextReader4.Close();
					Program.CredentialsProfiles.ReadXml(xmlNode2, ProfileScope.Global, DefaultSettingsGroup.Instance, list);
				}
			}
			else
			{
				if (preferences.Settings.DefaultGroupSettings != null)
				{
					XmlNode firstChild = preferences.Settings.DefaultGroupSettings.Value.FirstChild;
					DefaultSettingsGroup.Instance.ReadXml(firstChild, list);
				}
				if (preferences.Settings.CredentialsProfiles != null)
				{
					XmlNode firstChild2 = preferences.Settings.CredentialsProfiles.Value.FirstChild;
					Program.CredentialsProfiles.ReadXml(firstChild2, ProfileScope.Global, DefaultSettingsGroup.Instance, list);
				}
			}
			Encryption.DecryptPasswords();
			if (list.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder("The following errors were encountered:").AppendLine().AppendLine();
				foreach (string item in list)
				{
					stringBuilder.AppendLine(item);
				}
				stringBuilder.AppendLine().Append("Your global configuration has not loaded completely. If it is saved it almost certainly means losing information. Continue?");
				DialogResult dialogResult = FormTools.ExclamationDialog(stringBuilder.ToString(), MessageBoxButtons.YesNo);
				if (dialogResult == DialogResult.No)
				{
					return null;
				}
			}
			return preferences;
		}

		public void LoadBuiltInGroups()
		{
			if (Settings.BuiltInGroups.Value != null)
			{
				GroupBase.SchemaVersion = 3;
				XmlNode firstChild = Settings.BuiltInGroups.Value.FirstChild;
				List<string> errors = new List<string>();
				foreach (XmlNode childNode in firstChild.ChildNodes)
				{
					IEnumerable<IBuiltInVirtualGroup> builtInVirtualGroups = Program.BuiltInVirtualGroups;
					Func<IBuiltInVirtualGroup, bool> predicate = (IBuiltInVirtualGroup v) => childNode.Name.Equals(v.XmlNodeName);
					builtInVirtualGroups.Where(predicate).FirstOrDefault()?.ReadXml(childNode, null, errors);
				}
			}
		}

		public override void Save()
		{
			if (NeedToSave)
			{
				using (StringWriter stringWriter = new StringWriter())
				{
					using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
					{
						DefaultSettingsGroup.Instance.WriteXml(xmlTextWriter);
						xmlTextWriter.Flush();
						xmlTextWriter.Close();
						XmlDocument xmlDocument = new XmlDocument();
						xmlDocument.LoadXml(stringWriter.ToString());
						Settings.DefaultGroupSettings.Value = xmlDocument.LastChild;
					}
				}
				using (StringWriter stringWriter2 = new StringWriter())
				{
					using (XmlTextWriter xmlTextWriter2 = new XmlTextWriter(stringWriter2))
					{
						Program.CredentialsProfiles.WriteXml(xmlTextWriter2, DefaultSettingsGroup.Instance);
						xmlTextWriter2.Flush();
						xmlTextWriter2.Close();
						XmlDocument xmlDocument2 = new XmlDocument();
						xmlDocument2.LoadXml(stringWriter2.ToString());
						Settings.CredentialsProfiles.Value = xmlDocument2.LastChild;
					}
				}
				CollectFilesToOpen();
				SerializeBuiltInGroups();
				SerializePluginSettings();
				string temporaryFileName = Helpers.GetTemporaryFileName(SettingsPath, ".new");
				using (XmlTextWriter xmlTextWriter3 = new XmlTextWriter(temporaryFileName, Encoding.UTF8))
				{
					xmlTextWriter3.Formatting = Formatting.Indented;
					xmlTextWriter3.WriteStartDocument();
					Settings.WriteXml(xmlTextWriter3, null);
					xmlTextWriter3.WriteEndDocument();
					xmlTextWriter3.Close();
					Helpers.MoveTemporaryToPermanent(temporaryFileName, SettingsPath, saveOld: false);
				}
				NeedToSave = false;
			}
		}

		private void SerializeBuiltInGroups()
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				XmlTextWriter tw = new XmlTextWriter(stringWriter);
				try
				{
					tw.WriteStartElement("groups");
					Program.BuiltInVirtualGroups.ForEach(delegate(IBuiltInVirtualGroup virtualGroup)
					{
						if (!string.IsNullOrEmpty(virtualGroup.XmlNodeName))
						{
							virtualGroup.WriteXml(tw, null);
						}
					});
					tw.WriteEndElement();
					tw.Flush();
					tw.Close();
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(stringWriter.ToString());
					Settings.BuiltInGroups.Value = xmlDocument.LastChild;
				}
				finally
				{
					if (tw != null)
					{
						((IDisposable)tw).Dispose();
					}
				}
			}
		}

		private void CollectFilesToOpen()
		{
			FilesToOpen = (from file in ServerTree.Instance.Nodes.OfType<FileGroup>()
				select file.Pathname).ToList();
		}

		private void SerializePluginSettings()
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				XmlTextWriter tw = new XmlTextWriter(stringWriter);
				try
				{
					tw.WriteStartElement("plugins");
					Program.PluginAction(delegate(IPlugin p)
					{
						try
						{
							XmlNode xmlNode = p.SaveSettings();
							if (xmlNode != null)
							{
								tw.WriteStartElement("plugin");
								tw.WriteAttributeString("path", p.GetType().AssemblyQualifiedName);
								xmlNode.WriteTo(tw);
								tw.WriteEndElement();
							}
						}
						catch
						{
						}
					});
					tw.WriteEndElement();
					tw.Flush();
					tw.Close();
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(stringWriter.ToString());
					Settings.PluginSettings.Value = xmlDocument.LastChild;
				}
				finally
				{
					if (tw != null)
					{
						((IDisposable)tw).Dispose();
					}
				}
			}
		}

		internal bool GetBuiltInGroupVisibility(IBuiltInVirtualGroup builtInGroup)
		{
			string propertyName = $"Show{builtInGroup.ConfigPropertyName}Group";//builtInGroup.ConfigPropertyName
			return (bool)this[propertyName];
		}

		internal void SetBuiltInGroupVisibility(IBuiltInVirtualGroup builtInGroup, bool value)
		{
			string propertyName = $"Show{builtInGroup.ConfigPropertyName}Group";
			this[propertyName] = value;
		}
	}
}

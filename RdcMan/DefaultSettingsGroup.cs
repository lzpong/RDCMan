using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan {
	internal class DefaultSettingsGroup : VirtualGroup {
		//public new const string Name = "默认设置";

		//private const string XmlTopNode = "defaultSettings";

		public static DefaultSettingsGroup Instance { get; private set; }

		public override EncryptionSettings EncryptionSettings { get; protected set; }

		static DefaultSettingsGroup() {
			Instance = new DefaultSettingsGroup();
		}

		private DefaultSettingsGroup() {
			EncryptionSettings = new EncryptionSettings();
			base.AllSettingsGroups.Add(EncryptionSettings);
			foreach (SettingsGroup allSettingsGroup in base.AllSettingsGroups) {
				allSettingsGroup.InheritSettingsType.Mode = InheritanceMode.Disabled;
			}
		}

		internal override void ReadXml(XmlNode node, ICollection<string> errors) {
			if (!node.Name.Equals("defaultSettings")) {
				errors.Add("默认设置格式错误");
				return;
			}
			foreach (XmlNode childNode in node.ChildNodes) {
				ReadXmlSettingsGroup(childNode, errors);
			}
		}

		internal override void WriteXml(XmlTextWriter tw) {
			tw.WriteStartElement("defaultSettings");
			WriteXmlSettingsGroups(tw);
			tw.WriteEndElement();
		}

		public override void DoPropertiesDialog(Form parentForm, string activeTabName) {
			using DefaultGroupPropertiesDialog defaultGroupPropertiesDialog = DefaultGroupPropertiesDialog.NewPropertiesDialog(this, parentForm);
			defaultGroupPropertiesDialog.SetActiveTab(activeTabName);
			if (defaultGroupPropertiesDialog.ShowDialog() == DialogResult.OK) {
				UpdateSettings(defaultGroupPropertiesDialog);
				ServerTree.Instance.OnGroupChanged(ServerTree.Instance.RootNode, ChangeType.PropertyChanged);
				Program.Preferences.NeedToSave = true;
			}
		}
	}
}

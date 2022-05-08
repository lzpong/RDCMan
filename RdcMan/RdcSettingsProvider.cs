using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan {
	public class RdcSettingsProvider : SettingsProvider {
		//private const string SettingsRoot = "Settings";

		public override string ApplicationName {
			get {
				return "RDCMan";
			}
			set { }
		}

		private string SettingsDirectory => new FileInfo(Application.LocalUserAppDataPath).DirectoryName;

		private string SettingsFilename => Path.Combine(SettingsDirectory, ApplicationName + ".settings");

		public override void Initialize(string name, NameValueCollection values) {
			base.Initialize(ApplicationName, values);
		}

		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties) {
			SettingsPropertyValueCollection settingsPropertyValueCollection = new SettingsPropertyValueCollection();
			XmlDocument xmlDocument = new XmlDocument();
			XmlNode xmlNode = null;
			try {
				if (!Program.ResetPreferences) {
					xmlDocument.Load(SettingsFilename);
					xmlNode = xmlDocument.SelectSingleNode("Settings");
				}
			}
			catch { }
			if (xmlNode == null)
				xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, "root", "");

			foreach (SettingsProperty property in properties) {
				SettingsPropertyValue settingsPropertyValue = new SettingsPropertyValue(property);
				XmlNode xmlNode2 = xmlNode.SelectSingleNode(property.Name);
				if (xmlNode2 != null) {
					if (property.PropertyType == typeof(XmlDocument))
						settingsPropertyValue.SerializedValue = xmlNode2.InnerXml;
					else
						settingsPropertyValue.SerializedValue = xmlNode2.InnerText;
				}
				else
					settingsPropertyValue.SerializedValue = property.DefaultValue;

				settingsPropertyValueCollection.Add(settingsPropertyValue);
			}
			return settingsPropertyValueCollection;
		}

		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values) {
			throw new InvalidOperationException();
		}
	}
}

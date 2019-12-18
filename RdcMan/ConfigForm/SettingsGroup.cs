using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	public abstract class SettingsGroup
	{
		public string Name { get; private set; }

		public string XmlNodeName { get; private set; }

		public InheritSettingsType InheritSettingsType { get; private set; }

		protected abstract Dictionary<string, SettingProperty> SettingProperties { get; }

		internal SettingsGroup(string name, string xmlNodeName)
		{
			XmlNodeName = xmlNodeName;
			Name = name;
			InheritSettingsType = new InheritSettingsType();
			foreach (SettingProperty value2 in SettingProperties.Values)
			{
				ISetting value = (ISetting)Activator.CreateInstance(value2.Property.PropertyType, value2.Attribute.DefaultValue);
				value2.Property.SetValue(this, value, null);
			}
		}

		public abstract TabPage CreateTabPage(TabbedSettingsDialog dialog);

		public override string ToString() { return Name; }

		internal void ReadXml(XmlNode xmlNode, RdcTreeNode node, ICollection<string> errors)
		{
			InheritanceMode result = InheritSettingsType.Mode;
			if (result != InheritanceMode.Disabled)
			{
				try
				{
					string value = xmlNode.Attributes["inherit"].Value;
					if (!Enum.TryParse(value, out result))
					{
						errors.Add("Unexpected inheritance mode '{0}' in {1}".InvariantFormat(value, xmlNode.GetFullPath()));
						result = InheritanceMode.None;
					}
				}
				catch
				{
					errors.Add("No inheritance mode specified in {0}".InvariantFormat(xmlNode.GetFullPath()));
					result = InheritanceMode.None;
				}
				InheritSettingsType.Mode = result;
			}
			switch (result)
			{
			case InheritanceMode.FromParent:
			{
				bool anyInherited = false;
				InheritSettings(node, ref anyInherited);
				break;
			}
			case InheritanceMode.None:
			case InheritanceMode.Disabled:
				foreach (XmlNode childNode in xmlNode.ChildNodes)
				{
					try
					{
						ISetting setting = (ISetting)SettingProperties[childNode.Name].Property.GetValue(this, null);
						try
						{
							setting.ReadXml(childNode, node);
						}
						catch
						{
							errors.Add("Error processing Xml node {0}".InvariantFormat(childNode.GetFullPath()));
						}
					}
					catch
					{
						errors.Add("Unexpected Xml node {0}".InvariantFormat(childNode.GetFullPath()));
					}
				}
				break;
			default:
				errors.Add("Unexpected inheritance mode '{0}' in {1}".InvariantFormat(result.ToString(), xmlNode.GetFullPath()));
				break;
			}
		}

		internal void WriteXml(XmlTextWriter tw, RdcTreeNode node)
		{
			if (InheritSettingsType.Mode != 0)
			{
				tw.WriteStartElement(XmlNodeName);
				if (InheritSettingsType.Mode != InheritanceMode.Disabled)
				{
					InheritSettingsType.WriteXml(tw);
				}
				WriteSettings(tw, node);
				tw.WriteEndElement();
			}
		}

		protected virtual void WriteSettings(XmlTextWriter tw, RdcTreeNode node)
		{
			WriteSettings(tw, node, null);
		}

		protected virtual void WriteSettings(XmlTextWriter tw, RdcTreeNode node, HashSet<ISetting> exclusionSet)
		{
			foreach (SettingProperty value in SettingProperties.Values)
			{
				ISetting setting = (ISetting)value.Property.GetValue(this, null);
				if ((exclusionSet == null || !exclusionSet.Contains(setting)) && !value.Attribute.IsObsolete)
				{
					tw.WriteStartElement(value.Attribute.XmlName);
					setting.WriteXml(tw, node);
					tw.WriteEndElement();
				}
			}
		}

		internal void InheritSettings(RdcTreeNode node, ref bool anyInherited)
		{
			GroupBase inheritedSettingsNode = InheritSettingsType.GetInheritedSettingsNode(node);
			if (inheritedSettingsNode != null)
			{
				inheritedSettingsNode.InheritSettings();
				Copy(inheritedSettingsNode);
				anyInherited = true;
			}
		}

		protected virtual void Copy(RdcTreeNode node)
		{
			throw new NotImplementedException();
		}

		internal void Copy(SettingsGroup source)
		{
			foreach (SettingProperty value in SettingProperties.Values)
			{
				ISetting setting = (ISetting)value.Property.GetValue(this, null);
				ISetting source2 = (ISetting)value.Property.GetValue(source, null);
				setting.Copy(source2);
			}
		}
	}
}

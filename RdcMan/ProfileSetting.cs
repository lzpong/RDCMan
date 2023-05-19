using System.Xml;

namespace RdcMan
{
	public class ProfileSetting : StringSetting
	{
		public const string ProfileScopeAttribute = "scope";

		public ProfileScope Scope { get; private set; }

		public ProfileSetting(object o)
			: base(o)
		{
			Reset();
		}

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node)
		{
			base.ReadXml(xmlNode, node);
			try
			{
				XmlNode namedItem = xmlNode.Attributes.GetNamedItem(ProfileScopeAttribute);
				Scope = namedItem.InnerText.ParseEnum<ProfileScope>();
			}
			catch
			{
				Scope = ProfileScope.Local;
			}
		}

		public override void WriteXml(XmlTextWriter tw, RdcTreeNode node)
		{
			tw.WriteAttributeString(ProfileScopeAttribute, Scope.ToString());
			tw.WriteString(base.Value);
		}

		public override void Copy(ISetting source)
		{
			base.Copy(source);
			Scope = ((ProfileSetting)source).Scope;
		}

		public override string ToString()
		{
			return "{0} ({1})".InvariantFormat(base.Value, Scope);
		}

		public void UpdateValue(string newValue, ProfileScope newScope)
		{
			base.Value = newValue;
			Scope = newScope;
		}

		public void Reset()
		{
			Scope = ProfileScope.Local;
			base.Value = "Custom";
		}
	}
}

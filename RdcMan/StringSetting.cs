using System.Xml;

namespace RdcMan
{
	public class StringSetting : Setting<string>
	{
		public StringSetting(object o)
			: base(o)
		{
			if (base.Value == null)
			{
				base.Value = string.Empty;
			}
		}

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node)
		{
			xmlNode = xmlNode.FirstChild;
			if (xmlNode == null)
			{
				base.Value = string.Empty;
			}
			else
			{
				base.Value = xmlNode.InnerText;
			}
		}
	}
}

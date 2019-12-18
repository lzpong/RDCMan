using System.Xml;

namespace RdcMan
{
	public class BoolSetting : Setting<bool>
	{
		public BoolSetting(object o)
			: base(o)
		{
		}

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node)
		{
			base.Value = bool.Parse(xmlNode.FirstChild.InnerText);
		}
	}
}

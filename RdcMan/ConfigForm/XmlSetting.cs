using System.Xml;

namespace RdcMan
{
	public class XmlSetting : Setting<XmlNode>
	{
		public XmlSetting(object o)
			: base(o)
		{
		}

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node)
		{
			base.Value = xmlNode;
		}

		public override void WriteXml(XmlTextWriter tw, RdcTreeNode node)
		{
			base.Value.WriteTo(tw);
		}
	}
}

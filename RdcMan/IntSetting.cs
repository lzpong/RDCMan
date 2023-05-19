using System.Xml;

namespace RdcMan
{
	public class IntSetting : Setting<int>
	{
		public IntSetting(object o) : base(o) { }

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node)
		{
			base.Value = int.Parse(xmlNode.FirstChild.InnerText);
		}
	}
}

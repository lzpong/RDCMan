using System.Drawing;
using System.Xml;

namespace RdcMan
{
	public class PointSetting : Setting<Point>
	{
		public PointSetting(object o)
			: base(o)
		{
		}

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node)
		{
			base.Value = PointHelper.Parse(xmlNode.FirstChild.InnerText);
		}

		public override void WriteXml(XmlTextWriter tw, RdcTreeNode node)
		{
			tw.WriteString(base.Value.ToFormattedString());
		}

		public override string ToString()
		{
			return base.Value.ToFormattedString();
		}
	}
}

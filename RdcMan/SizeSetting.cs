using System.Drawing;
using System.Xml;

namespace RdcMan {
	public class SizeSetting : Setting<Size> {
		public SizeSetting(object o) : base(o) { }

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node) {
			base.Value = SizeHelper.Parse(xmlNode.FirstChild.InnerText);
		}

		public override void WriteXml(XmlTextWriter tw, RdcTreeNode node) {
			tw.WriteString(base.Value.ToFormattedString());
		}

		public override string ToString() {
			return base.Value.ToFormattedString();
		}
	}
}

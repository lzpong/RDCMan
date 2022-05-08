using System.Xml;

namespace RdcMan {
	public interface ISetting {
		void ReadXml(XmlNode xmlNode, RdcTreeNode node);

		void WriteXml(XmlTextWriter tw, RdcTreeNode node);

		void Copy(ISetting source);
	}
}

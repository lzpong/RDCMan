using System.Xml;

namespace RdcMan {
	public class PasswordSetting : BaseSetting<string>, IDeferDecryption {

		public bool IsDecrypted { get; set; }

		public PasswordSetting(object o)
			: base(o) { }

		public void SetPlainText(string value) {
			Value = value;
			IsDecrypted = true;
		}

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node) {
			XmlNode firstChild = xmlNode.FirstChild;
			if (firstChild == null)
				Value = string.Empty;
			else
				Value = firstChild.InnerText;

			try {
				XmlNode xmlNode2 = xmlNode.Attributes["storeAsClearText"];
				if (xmlNode2 != null && bool.Parse(xmlNode2.InnerText))
					node.Password.IsDecrypted = true;
				else if (xmlNode.ParentNode.Name != "credentialsProfile")
					Encryption.DeferDecryption(this, node, xmlNode.GetFullPath());
			}
			catch { }
		}

		public override void WriteXml(XmlTextWriter tw, RdcTreeNode node) {
			string text = (IsDecrypted ? Encryption.EncryptString(Value, node.EncryptionSettings) : Value);
			tw.WriteString(text);
		}

		public override void Copy(ISetting source) {
			base.Copy(source);
			IsDecrypted = ((PasswordSetting)source).IsDecrypted;
		}

		public void Decrypt(EncryptionSettings settings) {
			Value = Encryption.DecryptString(Value, settings);
			IsDecrypted = true;
		}
	}
}

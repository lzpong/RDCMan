using System.Xml;

namespace RdcMan {
	public abstract class BaseSetting<T> : ISetting {
		internal T Value;

		protected BaseSetting(object o) {
			Value = ((o != null) ? ((T)o) : default(T));
		}

		public abstract void ReadXml(XmlNode xmlNode, RdcTreeNode node);

		public virtual void WriteXml(XmlTextWriter tw, RdcTreeNode node) {
			tw.WriteString(Value.ToString());
		}

		public virtual void Copy(ISetting source) {
			Value = ((BaseSetting<T>)source).Value;
		}

		public override string ToString() {
			return Value.ToString();
		}
	}
}

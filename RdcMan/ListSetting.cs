using System;
using System.Collections.Generic;
using System.Xml;

namespace RdcMan {
	public class ListSetting<T> : Setting<List<T>> where T : class {
		//private const string XmlNodeName = "item";

		public ListSetting(object o)
			: base(o) {
			if (base.Value == null)
				base.Value = new List<T>();
		}

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node) {
			List<T> list = new List<T>();
			foreach (XmlNode childNode in xmlNode.ChildNodes) {
				if (childNode.Name != "item")
					throw new Exception();
				list.Add(childNode.InnerText as T);
			}
			base.Value = list;
		}

		public override void WriteXml(XmlTextWriter tw, RdcTreeNode node) {
			foreach (T item in base.Value) {
				tw.WriteElementString("item", item.ToString());
			}
		}
	}
}

using System;

namespace RdcMan {
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public class SettingAttribute : Attribute {
		public string XmlName { get; set; }

		public object DefaultValue { get; set; }

		public bool IsObsolete { get; set; }

		public SettingAttribute(string xmlName) {
			XmlName = xmlName;
		}
	}
}

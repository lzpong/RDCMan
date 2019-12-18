using System;
using System.Xml;

namespace RdcMan
{
	public class EnumSetting<TEnum> : Setting<TEnum> where TEnum : struct
	{
		public EnumSetting(object o)
			: base(o)
		{
		}

		public override void ReadXml(XmlNode xmlNode, RdcTreeNode node)
		{
			if (int.TryParse(xmlNode.InnerText, out int result))
			{
				base.Value = (TEnum)(object)result;
			}
			else
			{
				base.Value = (TEnum)Enum.Parse(typeof(TEnum), xmlNode.InnerText);
			}
		}

		public override void WriteXml(XmlTextWriter tw, RdcTreeNode node)
		{
			tw.WriteString(base.Value.ToString());
		}
	}
}

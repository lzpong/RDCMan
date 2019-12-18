using System;
using System.Xml;

namespace RdcMan
{
	public class InheritSettingsType
	{
		public InheritanceMode Mode
		{
			get;
			set;
		}

		public InheritSettingsType()
		{
			Mode = InheritanceMode.FromParent;
		}

		public GroupBase GetInheritedSettingsNode(RdcTreeNode node)
		{
			switch (Mode)
			{
			case InheritanceMode.None:
			case InheritanceMode.Disabled:
				return null;
			case InheritanceMode.FromParent:
				if (node.Parent != null)
				{
					return node.Parent as GroupBase;
				}
				return DefaultSettingsGroup.Instance;
			default:
				throw new Exception("Unexpected inheritance kind");
			}
		}

		public void WriteXml(XmlTextWriter tw)
		{
			tw.WriteAttributeString("inherit", Mode.ToString());
		}
	}
}

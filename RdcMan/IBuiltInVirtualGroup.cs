using System.Collections.Generic;
using System.Xml;

namespace RdcMan
{
	internal interface IBuiltInVirtualGroup
	{
		string XmlNodeName { get; }

		string Text { get; }

		string ConfigPropertyName { get; }

		bool IsInTree { get; set; }

		bool IsVisibilityConfigurable { get; }

		void ReadXml(XmlNode xmlNode, FileGroup fileGroup, ICollection<string> errors);

		void WriteXml(XmlTextWriter tw, FileGroup fileGroup);

		bool ShouldWriteNode(ServerRef serverRef, FileGroup file);
	}
}

using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	public interface IPlugin
	{
		void PreLoad(IPluginContext context, XmlNode xmlNode);

		void PostLoad(IPluginContext context);

		XmlNode SaveSettings();

		void Shutdown();

		void OnContextMenu(ContextMenuStrip contextMenuStrip, RdcTreeNode node);

		void OnUndockServer(IUndockedServerForm form);

		void OnDockServer(ServerBase server);
	}
}

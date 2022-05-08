using System.Windows.Forms;

namespace RdcMan {
	public static class MenuExtensions {
		public static ToolStripMenuItem Add(this ToolStrip menu, string text, MenuNames nameConstant) {
			return menu.Items.Add(text, nameConstant);
		}

		public static ToolStripMenuItem Add(this ToolStripItemCollection menuItems, string text, MenuNames nameConstant) {
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(text);
			toolStripMenuItem.Name = nameConstant.ToString();
			menuItems.Add(toolStripMenuItem);
			return toolStripMenuItem;
		}
	}
}

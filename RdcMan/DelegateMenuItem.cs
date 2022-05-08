using System;
using System.Windows.Forms;

namespace RdcMan {
	public class DelegateMenuItem : ToolStripMenuItem {
		public DelegateMenuItem(string text, MenuNames name, Action click)
			: base(text) {
			base.Click += delegate {
				click();
			};
			base.Name = name.ToString();
		}

		public DelegateMenuItem(string text, MenuNames name, string shortcut, Action click)
			: this(text, name, click) {
			base.ShortcutKeyDisplayString = shortcut;
		}
	}
}

using System;
using System.Windows.Forms;

namespace RdcMan
{
	public class DelegateMenuItem : ToolStripMenuItem
	{
		public DelegateMenuItem(string text, MenuNames name, Action click)
			: base(text)
		{
			EventHandler value = delegate
			{
				click();
			};
			base.Click += value;
			base.Name = name.ToString();
		}

		public DelegateMenuItem(string text, MenuNames name, string shortcut, Action click)
			: this(text, name, click)
		{
			base.ShortcutKeyDisplayString = shortcut;
		}
	}
}

using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan
{
	public static class ControlExtensions
	{
		public static IEnumerable<Control> FlattenControls(this Control.ControlCollection controls)
		{
			foreach (Control c in controls)
			{
				yield return c;
				if (c.Controls.Count <= 0)
				{
					continue;
				}
				foreach (Control item in c.Controls.FlattenControls())
				{
					yield return item;
				}
			}
		}
	}
}

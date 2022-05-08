using System.Text;
using System.Windows.Forms;

namespace RdcMan
{
	internal class SendKeysMenuItem : ToolStripMenuItem
	{
		public Keys[] KeyCodes;

		public SendKeysMenuItem(string name, Keys[] keyCodes)
		{
			KeyCodes = keyCodes;
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < keyCodes.Length; i++)
			{
				Keys keys = keyCodes[i];
				if (stringBuilder.Length > 0)
					stringBuilder.Append("+");

				switch (keys)
				{
				case Keys.ControlKey:
					stringBuilder.Append("Control");
					break;
				case Keys.ShiftKey:
					stringBuilder.Append("Shift");
					break;
				case Keys.Menu:
					stringBuilder.Append("Alt");
					break;
				default:
					stringBuilder.Append(keys.ToString());
					break;
				}
			}
			if (name != null)
				Text = name + " (" + stringBuilder.ToString() + ")";
			else
				Text = stringBuilder.ToString();
		}
	}
}

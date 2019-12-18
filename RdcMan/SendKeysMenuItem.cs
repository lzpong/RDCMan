using System.Text;
using System.Windows.Forms;

namespace RdcMan
{
	internal class SendKeysMenuItem : ToolStripMenuItem
	{
		public Keys[] KeyCodes;

		public SendKeysMenuItem(string text, Keys[] keyCodes)
		{
			KeyCodes = keyCodes;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Keys keys in keyCodes)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append("+");
				}
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
			if (text != null)
			{
				Text = text + " (" + stringBuilder.ToString() + ")";
			}
			else
			{
				Text = stringBuilder.ToString();
			}
		}
	}
}

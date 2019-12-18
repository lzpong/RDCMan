using System;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	public class ServerBox : Label
	{
		private Server _server;

		public ServerBox(Server server)
		{
			_server = server;
			BackColor = Color.White;
			BorderStyle = BorderStyle.FixedSingle;
			TextAlign = ContentAlignment.MiddleCenter;
			Hide();
		}

		public void SetText()
		{
			string text = _server.GetConnectionStateText();
			if (_server.IsClientUndocked)
			{
				text = text + Environment.NewLine + "{ Undocked }";
			}
			Text = text;
		}
	}
}

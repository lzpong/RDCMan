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
		/// <summary>
		/// Î´Í£¿¿Ê±Ë«»÷¼¤»îÔ¶³Ì×ÀÃæÏÔÊ¾  lzpong 2023/05/19
		/// </summary>
		/// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                if (e.Clicks == 1) {
                    //Focus();
                    return;
                }
                ServerTree.Instance.SelectedNode = _server;
                _server.Connect();
                _server.Focus();
            }
        }

		public void DbClickShow() {
            ServerTree.Instance.SelectedNode = _server;
            _server.Connect();
            _server.Focus();
        }

        public void SetText()
		{
			string text = _server.GetConnectionStateText();
			if (_server.IsClientUndocked)
				text = text + Environment.NewLine + "{ Î´Í£¿¿ }";

			Text = text;
		}
	}
}

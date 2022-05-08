using System.ComponentModel;
using System.Windows.Forms;
using AxMSTSCLib;

namespace RdcMan {
	internal class RdpClient5 : AxMsRdpClient5, IRdpClient {
		public RdpClient5(MainForm form) {
			((ISupportInitialize)this).BeginInit();
			Hide();
			form.AddToClientPanel(this);
			((ISupportInitialize)this).EndInit();
		}

		protected override void WndProc(ref Message m) {
			if ((long)m.Msg == 33 && !base.ContainsFocus)
				Focus();

			base.WndProc(ref m);
		}
	}
}

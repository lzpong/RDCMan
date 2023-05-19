using System.ComponentModel;
using AxMSTSCLib;

namespace RdcMan
{
	internal class RdpClient9 : AxMsRdpClient9NotSafeForScripting, IRdpClient
	{
		public RdpClient9(MainForm form)
		{
			((ISupportInitialize)this).BeginInit();
			Hide();
			form.AddToClientPanel(this);
			((ISupportInitialize)this).EndInit();
		}
	}
}

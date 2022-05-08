using System.ComponentModel;
using AxMSTSCLib;

namespace RdcMan
{
	internal class RdpClient10 : AxMsRdpClient10NotSafeForScripting, IRdpClient
	{
		public RdpClient10(MainForm form)
		{
			((ISupportInitialize)this).BeginInit();
			Hide();
			form.AddToClientPanel(this);
			((ISupportInitialize)this).EndInit();
		}
	}
}

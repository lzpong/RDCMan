using System.Windows.Forms;

namespace RdcMan
{
	public class ConnectToDialog : QuickConnectDialog
	{
		public TemporaryServer Server { get; private set; }

		private ConnectToDialog(string title, string buttonText, Form parentForm)
			: base(title, buttonText, parentForm)
		{
		}

		public static ConnectToDialog NewConnectToDialog(Form parentForm)
		{
			ConnectToDialog connectToDialog = new ConnectToDialog("连接到", "连接(&C)", parentForm);
			connectToDialog.Server = TemporaryServer.CreateForQuickConnect();
			connectToDialog.CreateControls(inputServerName: true, connectToDialog.Server.LogonCredentials, connectToDialog.Server.ConnectionSettings, null);
			return connectToDialog;
		}
	}
}

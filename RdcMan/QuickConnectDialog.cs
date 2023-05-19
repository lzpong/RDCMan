using System;
using System.Windows.Forms;

namespace RdcMan
{
	public class QuickConnectDialog : TabbedSettingsDialog
	{
		public QuickConnectTabPage QuickConnectTabPage { get; private set; }

		protected QuickConnectDialog(string title, string buttonText, Form parentForm)
			: base(title, buttonText, parentForm) { }

		public void CreateControls(bool inputServerName, LogonCredentials logonCredentials, ConnectionSettings connectionSettings, FileGroup fileGroup)
		{
			QuickConnectTabPage = new QuickConnectTabPage(this, logonCredentials);
			QuickConnectTabPage.CreateControls(inputServerName, fileGroup);
			AddTabPage(QuickConnectTabPage);
			connectionSettings.InheritSettingsType.Mode = InheritanceMode.Disabled;
			AddTabPage(connectionSettings.CreateTabPage(this));
			InitButtons();
		}

		protected override void ShownCallback(object sender, EventArgs e)
		{
			base.ShownCallback(sender, e);
			QuickConnectTabPage.OnShown();
		}
	}
}

using System.Linq;
using System.Windows.Forms;

namespace RdcMan
{
	public abstract class NodePropertiesDialog : TabbedSettingsDialog
	{
		public RdcTreeNode AssociatedNode { get; private set; }

		public INodePropertiesPage PropertiesPage { get; protected set; }

		public override void InitButtons()
		{
			base.InitButtons();
			if (AssociatedNode != null)
			{
				_acceptButton.Enabled = AssociatedNode.AllowEdit(popUI: true);
			}
		}

		protected NodePropertiesDialog(RdcTreeNode associatedNode, string dialogTitle, string acceptButtonText, Form parentForm)
			: base(dialogTitle, acceptButtonText, parentForm)
		{
			AssociatedNode = associatedNode;
		}

		public virtual void CreateControls(RdcTreeNode settings)
		{
			LogonCredentialsTabPage logonCredentialsTabPage = (LogonCredentialsTabPage)settings.LogonCredentials.CreateTabPage(this);
			LogonCredentialsDialogOptions logonCredentialsDialogOptions = LogonCredentialsDialogOptions.ShowProfiles;
			if (settings.LogonCredentials.InheritSettingsType.Mode != InheritanceMode.Disabled)
			{
				logonCredentialsDialogOptions |= LogonCredentialsDialogOptions.AllowInheritance;
			}
			logonCredentialsTabPage.CreateControls(logonCredentialsDialogOptions);
			AddTabPage(logonCredentialsTabPage);
			GatewaySettingsTabPage gatewaySettingsTabPage = (GatewaySettingsTabPage)settings.GatewaySettings.CreateTabPage(this);
			logonCredentialsDialogOptions = LogonCredentialsDialogOptions.ShowProfiles;
			if (settings.GatewaySettings.InheritSettingsType.Mode != InheritanceMode.Disabled)
			{
				logonCredentialsDialogOptions |= LogonCredentialsDialogOptions.AllowInheritance;
			}
			gatewaySettingsTabPage.CreateControls(logonCredentialsDialogOptions);
			AddTabPage(gatewaySettingsTabPage);
			AddTabPage(settings.ConnectionSettings.CreateTabPage(this));
			AddTabPage(settings.RemoteDesktopSettings.CreateTabPage(this));
			AddTabPage(settings.LocalResourceSettings.CreateTabPage(this));
			AddTabPage(settings.SecuritySettings.CreateTabPage(this));
			AddTabPage(settings.DisplaySettings.CreateTabPage(this));
			InitButtons();
			this.ScaleAndLayout();
			settings.InheritSettings();
			settings.ResolveCredentials();
		}

		protected void PopulateCredentialsProfiles(GroupBase group)
		{
			FileGroup file = group?.FileGroup;
			foreach (ICredentialsTabPage item in base.TabPages.OfType<ICredentialsTabPage>())
			{
				item.PopulateCredentialsProfiles(file);
			}
		}
	}
}

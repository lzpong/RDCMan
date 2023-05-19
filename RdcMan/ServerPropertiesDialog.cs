using System.Windows.Forms;

namespace RdcMan
{
	internal class ServerPropertiesDialog : NodePropertiesDialog
	{
		private ServerPropertiesDialog(Server server, string dialogTitle, string acceptButtonText, Form parentForm)
			: base(server, dialogTitle, acceptButtonText, parentForm)
		{
		}

		private void CreateServerPropertiesPage(RdcTreeNode settings)
		{
			ServerPropertiesTabPage page = (ServerPropertiesTabPage)(base.PropertiesPage = settings.Properties.CreateTabPage(this) as ServerPropertiesTabPage);
			AddTabPage(page);
			base.PropertiesPage.ParentGroupChanged += base.PopulateCredentialsProfiles;
		}

		private void CreateImportServersPage(RdcTreeNode settings)
		{
			ImportServersPropertiesPage page = (ImportServersPropertiesPage)(base.PropertiesPage = new ImportServersPropertiesPage(this));
			AddTabPage(page);
			base.PropertiesPage.ParentGroupChanged += base.PopulateCredentialsProfiles;
		}

		public static ServerPropertiesDialog NewAddDialog(GroupBase parent)
		{
			Server server = Server.CreateForAddDialog();
			ServerPropertiesDialog serverPropertiesDialog = new ServerPropertiesDialog(server, "添加服务器", "添加", null);
			serverPropertiesDialog.CreateServerPropertiesPage(server);
			serverPropertiesDialog.CreateControls(server);
			if (!serverPropertiesDialog.PropertiesPage.PopulateParentDropDown(null, parent))
			{
				serverPropertiesDialog.Dispose();
				return null;
			}
			return serverPropertiesDialog;
		}

		public static ServerPropertiesDialog NewImportDialog(GroupBase parent)
		{
			Server server = Server.CreateForAddDialog();
			ServerPropertiesDialog serverPropertiesDialog = new ServerPropertiesDialog(server, "导入服务器", "导入", null);
			serverPropertiesDialog.CreateImportServersPage(server);
			serverPropertiesDialog.CreateControls(server);
			if (!serverPropertiesDialog.PropertiesPage.PopulateParentDropDown(null, parent))
			{
				serverPropertiesDialog.Dispose();
				return null;
			}
			return serverPropertiesDialog;
		}

		public static ServerPropertiesDialog NewPropertiesDialog(Server server, Form parentForm)
		{
			ServerPropertiesDialog serverPropertiesDialog = new ServerPropertiesDialog(server, server.DisplayName + " 服务器属性", "确定", parentForm);
			serverPropertiesDialog.CreateServerPropertiesPage(server);
			serverPropertiesDialog.CreateControls(server);
			if (server.FileGroup == null)
			{
				serverPropertiesDialog.PropertiesPage.SetParentDropDown(server.Parent as GroupBase);
			}
			serverPropertiesDialog.PropertiesPage.PopulateParentDropDown(null, server.Parent as GroupBase);
			return serverPropertiesDialog;
		}
	}
}

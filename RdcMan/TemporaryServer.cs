using System.Windows.Forms;

namespace RdcMan
{
	public class TemporaryServer : Server
	{
		protected TemporaryServer()
		{
		}

		public static TemporaryServer CreateForQuickConnect()
		{
			return new TemporaryServer();
		}

		public static TemporaryServer Create(ConnectToDialog dlg)
		{
			TemporaryServer server = dlg.Server;
			dlg.UpdateSettings();
			server.Properties.ServerName.Value = dlg.QuickConnectTabPage.ServerNameTextBox.Text;
			server.Properties.DisplayName.Value = dlg.QuickConnectTabPage.ServerNameTextBox.Text;
			server.LogonCredentials.InheritSettingsType.Mode = InheritanceMode.None;
			server.ConnectionSettings.InheritSettingsType.Mode = InheritanceMode.None;
			server.FinishConstruction(ConnectToGroup.Instance);
			ConnectToGroup.Instance.IsInTree = true;
			return server;
		}

		public override bool CanDropOnTarget(RdcTreeNode targetNode)
		{
			if (FileGroup == null)
			{
				GroupBase groupBase = (targetNode as GroupBase) ?? (targetNode.Parent as GroupBase);
				if (groupBase.DropBehavior() != DragDropEffects.Link)
				{
					return groupBase.CanDropServers();
				}
				return false;
			}
			return base.CanDropOnTarget(targetNode);
		}
	}
}

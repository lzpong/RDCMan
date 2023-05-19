using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	[Export(typeof(IBuiltInVirtualGroup))]
	internal class RecentlyUsedGroup : BuiltInVirtualGroup<RecentlyUsedServerRef>, IServerRefFactory
	{
		private class RecentlyUsedSettings : GroupSettings
		{
			private class RecentlyUsedSettingsTabPage : SettingsTabPage<RecentlyUsedSettings>
			{
				public RecentlyUsedSettingsTabPage(TabbedSettingsDialog dialog, RecentlyUsedSettings settings)
					: base(dialog, settings)
				{
					int rowIndex = 0;
					int num = 0;
					Label label = FormTools.NewLabel("条目数", 0, rowIndex);
					NumericTextBox numericTextBox = new NumericTextBox(1, 20, "条目数必须为 1 到 20") {
						Location = FormTools.NewLocation(1, rowIndex++),
						TabIndex = num++,
						TabStop = true,
						Setting = base.Settings.MaxNumberOfServers,
						Size = new Size(20, FormTools.ControlHeight)
					};
					base.Controls.Add(label, numericTextBox);
					base.FocusControl = numericTextBox;
				}
			}

			//private new const string TabName = "Properties";

			private static Dictionary<string, SettingProperty> _settingProperties;

			protected override Dictionary<string, SettingProperty> SettingProperties => _settingProperties;

			[Setting("maxNumberOfServers", DefaultValue = 10)]
			public IntSetting MaxNumberOfServers { get; private set; }

			static RecentlyUsedSettings()
			{
				typeof(RecentlyUsedSettings).GetSettingProperties(out _settingProperties);
			}

			public RecentlyUsedSettings()
				: base("属性")
			{
				base.InheritSettingsType.Mode = InheritanceMode.Disabled;
			}

			public override TabPage CreateTabPage(TabbedSettingsDialog dialog)
			{
				return new RecentlyUsedSettingsTabPage(dialog, this);
			}
		}

		public static RecentlyUsedGroup Instance { get; private set; }

		public override string ConfigName => "RecentlyUsed";

		public override bool AllowSort => false;

		public override bool HasProperties => true;

		protected override string XmlNodeName => "recentlyUsed";

		static RecentlyUsedGroup()
		{
			Server.ConnectionStateChanged += Server_ConnectionStateChanged;
			Server.FocusReceived += Server_FocusReceived;
		}

		private static void Server_FocusReceived(Server server)
		{
			if (server.ConnectionState == RdpClient.ConnectionState.Connected)
			{
				Instance.MoveToTop(server);
			}
		}

		private static void Server_ConnectionStateChanged(ConnectionStateChangedEventArgs args)
		{
			if (args.State == RdpClient.ConnectionState.Connected)
			{
				Instance.MoveToTop(args.Server);
			}
		}

		public override RecentlyUsedServerRef AddReference(ServerBase serverBase)
		{
			Server server = serverBase.ServerNode;
			RecentlyUsedServerRef serverRef = server.FindServerRef<RecentlyUsedServerRef>();
			if (serverRef == null)
			{
				ServerTree.Instance.Operation(OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate
				{
					serverRef = base.ServerRefFactory.Create(server) as RecentlyUsedServerRef;
					base.Nodes.Insert(0, serverRef);
					RemoveExtra();
				});
				ServerTree.Instance.OnGroupChanged(Instance, ChangeType.TreeChanged);
			}
			return serverRef;
		}

		private void RemoveExtra()
		{
			ServerTree.Instance.Operation(OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate
			{
				int value = (base.Properties as RecentlyUsedSettings).MaxNumberOfServers.Value;
				while (base.Nodes.Count > value)
				{
					ServerTree.Instance.RemoveNode(base.Nodes[value] as RdcTreeNode);
				}
			});
		}

		private void MoveToTop(Server server)
		{
			ServerRef serverRef = AddReference(server);
			if (serverRef.Index > 0)
			{
				ServerTree.Instance.Operation(OperationBehavior.RestoreSelected, delegate
				{
					base.Nodes.Remove(serverRef);
					base.Nodes.Insert(0, serverRef);
				});
				ServerTree.Instance.OnGroupChanged(Instance, ChangeType.InvalidateUI);
			}
		}

		private RecentlyUsedGroup()
		{
			base.Text = "最近的";
			Instance = this;
		}

		protected override void InitSettings()
		{
			((RdcTreeNode)this).Properties = new RecentlyUsedSettings();
			base.AllSettingsGroups.Add(base.Properties);
		}

		public override bool CanRemoveChildren()
		{
			return false;
		}

		public override void DoPropertiesDialog(Form parentForm, string activeTabName)
		{
			using TabbedSettingsDialog tabbedSettingsDialog = new TabbedSettingsDialog("最近使用的组设置", "确定", parentForm);
			tabbedSettingsDialog.AddTabPage(base.Properties.CreateTabPage(tabbedSettingsDialog));
			tabbedSettingsDialog.InitButtons();
			if (tabbedSettingsDialog.ShowDialog(parentForm) == DialogResult.OK)
			{
				tabbedSettingsDialog.UpdateSettings();
				RemoveExtra();
				ServerTree.Instance.OnGroupChanged(this, ChangeType.PropertyChanged);
			}
		}

		public ServerRef Create(Server server)
		{
			return new RecentlyUsedServerRef(server);
		}

		protected override bool ShouldWriteNode(RdcTreeNode node, FileGroup file)
		{
			return file == null;
		}
	}
}

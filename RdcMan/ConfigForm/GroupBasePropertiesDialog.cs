using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RdcMan
{
	internal abstract class GroupBasePropertiesDialog : NodePropertiesDialog
	{
		protected ListBox _credentialsListBox;

		private CredentialsStore _credentialsStore;

		private int _credentialsStoreChangeId;

		protected GroupBasePropertiesDialog(GroupBase group, string dialogTitle, string acceptButtonText, Form parentForm)
			: base(group, dialogTitle, acceptButtonText, parentForm)
		{
		}

		protected int CreateProfileManagementTabPage()
		{
			int result = 0;
			TabPage tabPage = FormTools.NewTabPage("配置管理");
			Label label = FormTools.NewLabel("配置", 0, result++);
			_credentialsListBox = new ListBox
			{
				Location = FormTools.NewLocation(0, result++),
				Size = new Size(340, 200)
			};
			_credentialsListBox.KeyDown += CredentialsListBox_KeyDown;
			_credentialsListBox.DoubleClick += EditButton_Click;
			_credentialsListBox.VisibleChanged += CredentialsListBox_VisibleChanged;
			Button button = new Button();
			button.Text = "添加";
			button.Location = new Point(_credentialsListBox.Right + 20, _credentialsListBox.Top);
			Button button2 = button;
			button2.Click += AddButton_Click;
			Button button3 = new Button();
			button3.Text = "编辑";
			button3.Location = new Point(_credentialsListBox.Right + 20, button2.Bottom + 4);
			Button button4 = button3;
			button4.Click += EditButton_Click;
			Button button5 = new Button();
			button5.Text = "删除";
			button5.Location = new Point(_credentialsListBox.Right + 20, button4.Bottom + 4);
			Button button6 = button5;
			button6.Click += DeleteButton_Click;
			tabPage.Controls.Add(label, _credentialsListBox, button2, button4, button6);
			tabPage.ResumeLayout();
			AddTabPage(tabPage);
			return result;
		}

		private void CredentialsListBox_VisibleChanged(object sender, EventArgs e)
		{
			PopulateCredentialsListBoxIfChanged();
		}

		private void PopulateCredentialsListBoxIfChanged()
		{
			if (_credentialsStoreChangeId != _credentialsStore.ChangeId)
			{
				PopulateCredentialsListBox();
			}
		}

		protected void PopulateCredentialsManagementTab(CredentialsStore store)
		{
			_credentialsStore = store;
			PopulateCredentialsListBox();
		}

		private void PopulateCredentialsListBox()
		{
			_credentialsListBox.Items.Clear();
			foreach (CredentialsProfile profile in _credentialsStore.Profiles)
			{
				_credentialsListBox.Items.Add(profile);
			}
			_credentialsStoreChangeId = _credentialsStore.ChangeId;
		}

		private void AddButton_Click(object sender, EventArgs e)
		{
			using (AddCredentialsDialog addCredentialsDialog = new AddCredentialsDialog(base.AssociatedNode))
			{
				if (addCredentialsDialog.ShowDialog() == DialogResult.OK)
				{
					CredentialsProfile credentialsProfile = new CredentialsProfile(addCredentialsDialog.ProfileName, addCredentialsDialog.ProfileScope, addCredentialsDialog.UserName, addCredentialsDialog.Password.Value, addCredentialsDialog.Domain);
					if (!_credentialsStore.Contains(credentialsProfile.ProfileName))
					{
						_credentialsListBox.Items.Add(credentialsProfile);
					}
					_credentialsStore[credentialsProfile.ProfileName] = credentialsProfile;
				}
			}
		}

		private void EditButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = _credentialsListBox.SelectedIndex;
			if (selectedIndex != -1)
			{
				CredentialsProfile credentialsProfile = _credentialsListBox.Items[selectedIndex] as CredentialsProfile;
				using (LogonSettingsDialog logonSettingsDialog = LogonSettingsDialog.NewEditCredentialsDialog(credentialsProfile))
				{
					if (logonSettingsDialog.ShowDialog() == DialogResult.OK)
					{
						PasswordSetting password = logonSettingsDialog.PasswordChanged ? logonSettingsDialog.Password : credentialsProfile.Password;
						credentialsProfile = new CredentialsProfile(credentialsProfile.ProfileName, credentialsProfile.ProfileScope, logonSettingsDialog.UserName, password, logonSettingsDialog.Domain);
						_credentialsStore[credentialsProfile.ProfileName] = credentialsProfile;
						_credentialsListBox.Items[selectedIndex] = credentialsProfile;
					}
				}
			}
		}

		private void DeleteButton_Click(object sender, EventArgs e)
		{
			DeleteCredentials();
		}

		private void CredentialsListBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				e.Handled = true;
				DeleteCredentials();
			}
		}

		private void DeleteCredentials()
		{
			int selectedIndex = _credentialsListBox.SelectedIndex;
			if (selectedIndex == -1)
			{
				return;
			}
			CredentialsProfile credentialsProfile = _credentialsListBox.Items[selectedIndex] as CredentialsProfile;
			ICollection<string> credentialsInUseLocations = GetCredentialsInUseLocations(credentialsProfile);
			if (credentialsInUseLocations.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(credentialsProfile.ProfileName + " 在这些节点中正在使用:").AppendLine();
				foreach (string item in credentialsInUseLocations)
				{
					stringBuilder.AppendLine(item);
				}
				stringBuilder.AppendLine().AppendLine("你确定你要删除它？");
				if (FormTools.YesNoDialog(stringBuilder.ToString()) != DialogResult.Yes)
				{
					return;
				}
			}
			_credentialsStore.Remove(credentialsProfile.ProfileName);
			_credentialsListBox.Items.RemoveAt(selectedIndex);
			RevertDeletedCredentials(credentialsProfile);
		}

		private ICollection<string> GetCredentialsInUseLocations(CredentialsProfile credentials)
		{
			HashSet<string> inUseLocations = new HashSet<string>();
			ICollection nodes = (base.AssociatedNode.FileGroup == null) ? ((IList)ServerTree.Instance.Nodes) : ((IList)new FileGroup[1]
			{
				base.AssociatedNode.FileGroup
			});
			foreach (TabPage tabPage in base.TabPages)
			{
				ICredentialsTabPage credentialsTabPage = tabPage as ICredentialsTabPage;
				if (credentialsTabPage != null && credentialsTabPage.Credentials == credentials)
				{
					inUseLocations.Add("{0}.{1}".InvariantFormat(Text, tabPage.Text));
				}
			}
			nodes.VisitNodes(delegate(RdcTreeNode node)
			{
				if (node is VirtualGroup)
				{
					return NodeVisitorResult.NoRecurse;
				}
				if (node.LogonCredentials.DirectlyReferences(credentials))
				{
					inUseLocations.Add("{0}.{1}".InvariantFormat(node.FullPath, "登录证书"));
				}
				if (node.GatewaySettings.DirectlyReferences(credentials))
				{
					inUseLocations.Add("{0}.{1}".InvariantFormat(node.FullPath, "网关配置"));
				}
				return NodeVisitorResult.Continue;
			});
			return inUseLocations;
		}

		private void RevertDeletedCredentials(CredentialsProfile credentials)
		{
			ICollection nodes = (base.AssociatedNode.FileGroup == null) ? ((IList)ServerTree.Instance.Nodes) : ((IList)new FileGroup[1]
			{
				base.AssociatedNode.FileGroup
			});
			nodes.VisitNodes(delegate(RdcTreeNode node)
			{
				if (node is VirtualGroup)
				{
					return NodeVisitorResult.NoRecurse;
				}
				if (node.LogonCredentials.DirectlyReferences(credentials))
				{
					node.LogonCredentials.ProfileName.Reset();
				}
				if (node.GatewaySettings.DirectlyReferences(credentials))
				{
					node.GatewaySettings.ProfileName.Reset();
				}
				return NodeVisitorResult.Continue;
			});
		}
	}
}

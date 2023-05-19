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
			TabPage tabPage = FormTools.NewTabPage("登录凭证管理");
			Label label = FormTools.NewLabel("登录凭证", 0, result++);
			_credentialsListBox = new ListBox
			{
				Location = FormTools.NewLocation(0, result++),
				Size = new Size(340, 200)
			};
			_credentialsListBox.KeyDown += CredentialsListBox_KeyDown;
			_credentialsListBox.DoubleClick += EditButton_Click;
			_credentialsListBox.VisibleChanged += CredentialsListBox_VisibleChanged;
			Button button = new Button
			{
				Text = "添加(&A)",
				Location = new Point(_credentialsListBox.Right + FormTools.ControlHeight, _credentialsListBox.Top)
			};
			button.Click += AddButton_Click;
			Button button2 = new Button
			{
				Text = "编辑(&E)",
				Location = new Point(_credentialsListBox.Right + FormTools.ControlHeight, button.Bottom + FormTools.VerticalSpace)
			};
			button2.Click += EditButton_Click;
			Button button3 = new Button
			{
				Text = "删除(&D)",
				Location = new Point(_credentialsListBox.Right + FormTools.ControlHeight, button2.Bottom + FormTools.VerticalSpace)
			};
			button3.Click += DeleteButton_Click;
			tabPage.Controls.Add(label, _credentialsListBox, button, button2, button3);
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
			using AddCredentialsDialog addCredentialsDialog = new AddCredentialsDialog(base.AssociatedNode);
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

		private void EditButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = _credentialsListBox.SelectedIndex;
			if (selectedIndex == -1)
			{
				return;
			}
			CredentialsProfile credentialsProfile = _credentialsListBox.Items[selectedIndex] as CredentialsProfile;
			using LogonSettingsDialog logonSettingsDialog = LogonSettingsDialog.NewEditCredentialsDialog(credentialsProfile);
			if (logonSettingsDialog.ShowDialog() == DialogResult.OK)
			{
				PasswordSetting password = (logonSettingsDialog.PasswordChanged ? logonSettingsDialog.Password : credentialsProfile.Password);
				credentialsProfile = new CredentialsProfile(credentialsProfile.ProfileName, credentialsProfile.ProfileScope, logonSettingsDialog.UserName, password, logonSettingsDialog.Domain);
				_credentialsStore[credentialsProfile.ProfileName] = credentialsProfile;
				_credentialsListBox.Items[selectedIndex] = credentialsProfile;
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
				stringBuilder.AppendLine(credentialsProfile.ProfileName + " 已在这些节点中使用：").AppendLine();
				foreach (string item in credentialsInUseLocations)
				{
					stringBuilder.AppendLine(item);
				}
				stringBuilder.AppendLine().AppendLine("确定要删除它？");
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
			ICollection nodes = ((base.AssociatedNode.FileGroup == null) ? ((IList)ServerTree.Instance.Nodes) : ((IList)new FileGroup[1] { base.AssociatedNode.FileGroup }));
			foreach (TabPage tabPage in base.TabPages)
			{
				if (tabPage is ICredentialsTabPage credentialsTabPage && credentialsTabPage.Credentials == credentials)
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
					inUseLocations.Add("{0}.{1}".InvariantFormat(node.FullPath, "登录凭证"));
				}
				if (node.GatewaySettings.DirectlyReferences(credentials))
				{
					inUseLocations.Add("{0}.{1}".InvariantFormat(node.FullPath, "网关设置"));
				}
				return NodeVisitorResult.Continue;
			});
			return inUseLocations;
		}

		private void RevertDeletedCredentials(CredentialsProfile credentials)
		{
			ICollection nodes = ((base.AssociatedNode.FileGroup == null) ? ((IList)ServerTree.Instance.Nodes) : ((IList)new FileGroup[1] { base.AssociatedNode.FileGroup }));
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

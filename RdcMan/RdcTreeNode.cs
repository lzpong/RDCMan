using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan
{
	public abstract class RdcTreeNode : TreeNode, ILogonCredentials
	{
		//public const string PropertiesXmlNodeName = "properties";

		private bool _needToUpdateInheritedSettings = true;

		public string ProfileName => LogonCredentials.ProfileName.Value;

		public ProfileScope ProfileScope => LogonCredentials.ProfileName.Scope;

		public string UserName => LogonCredentials.UserName.Value;

		public PasswordSetting Password => LogonCredentials.Password;

		public string Domain => LogonCredentials.Domain.Value;

		public virtual RdcBaseForm ParentForm => Program.TheForm;

		public CommonNodeSettings Properties { get; set; }

		public LogonCredentials LogonCredentials { get; private set; }

		public ConnectionSettings ConnectionSettings { get; private set; }

		public GatewaySettings GatewaySettings { get; private set; }

		public RemoteDesktopSettings RemoteDesktopSettings { get; private set; }

		public LocalResourcesSettings LocalResourceSettings { get; private set; }

		public CommonDisplaySettings DisplaySettings { get; set; }

		public SecuritySettings SecuritySettings { get; private set; }

		public virtual EncryptionSettings EncryptionSettings {
			get => FileGroup.EncryptionSettings;
			protected set => throw new NotImplementedException();
		}

		protected List<SettingsGroup> AllSettingsGroups { get; private set; }

		public virtual FileGroup FileGroup
		{
			get
			{
				TreeNode treeNode = this;
				while (treeNode.Parent != null)
				{
					treeNode = treeNode.Parent;
				}
				return treeNode as FileGroup;
			}
		}

		public string ParentPath {
			get => base.Parent == null ? ServerTree.Instance.RootNode.Text : base.Parent.FullPath;
		}

		public virtual bool HasProperties => true;

		internal GroupBase GetReadOnlyParent()
		{
			TreeNode treeNode = this;
			do
			{
				if (treeNode is GroupBase groupBase && groupBase.IsReadOnly)
				{
					return groupBase;
				}
				treeNode = treeNode.Parent;
			}
			while (treeNode != null);
			return null;
		}

		public List<TreeNode> GetPath()
		{
			List<TreeNode> list = new List<TreeNode>();
			this.VisitNodeAndParents(delegate(RdcTreeNode node)
			{
				list.Insert(0, node);
			});
			return list;
		}

		public int GetPathLength()
		{
			int len = 0;
			this.VisitNodeAndParents(delegate
			{
				len++;
			});
			return len;
		}

		protected RdcTreeNode()
		{
			AllSettingsGroups = new List<SettingsGroup>();
			InitSettings();
		}

		protected virtual void InitSettings()
		{
			LogonCredentials = new LogonCredentials();
			ConnectionSettings = new ConnectionSettings();
			GatewaySettings = new GatewaySettings();
			RemoteDesktopSettings = new RemoteDesktopSettings();
			LocalResourceSettings = new LocalResourcesSettings();
			SecuritySettings = new SecuritySettings();
			if (Properties != null)
			{
				AllSettingsGroups.Add(Properties);
			}
			AllSettingsGroups.AddRange(new SettingsGroup[7] { LogonCredentials, ConnectionSettings, GatewaySettings, RemoteDesktopSettings, LocalResourceSettings, DisplaySettings, SecuritySettings });
		}

		internal void CopySettings(RdcTreeNode node, Type excludeType)
		{
			for (int i = 0; i < AllSettingsGroups.Count; i++)
			{
				SettingsGroup settingsGroup = AllSettingsGroups[i];
				if (!(settingsGroup.GetType() == excludeType))
				{
					AllSettingsGroups[i].InheritSettingsType.Mode = node.AllSettingsGroups[i].InheritSettingsType.Mode;
					AllSettingsGroups[i].Copy(node.AllSettingsGroups[i]);
				}
			}
		}

		internal SettingsGroup GetSettingsGroupByName(string name)
		{
			return AllSettingsGroups.Where((SettingsGroup sg) => sg.Name.Equals(name)).FirstOrDefault();
		}

		internal abstract void WriteXml(XmlTextWriter tw);

		protected void WriteXmlSettingsGroups(XmlTextWriter tw)
		{
			foreach (SettingsGroup allSettingsGroup in AllSettingsGroups)
			{
				allSettingsGroup.WriteXml(tw, this);
			}
		}

		protected void ReadXml(Dictionary<string, Helpers.ReadXmlDelegate> nodeActions, XmlNode xmlNode, ICollection<string> errors)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				nodeActions.TryGetValue(childNode.Name, out var value);
				try
				{
					if (value != null)
					{
						value(childNode, this, errors);
					}
					else
					{
						ReadXmlSettingsGroup(childNode, errors);
					}
				}
				catch (Exception ex)
				{
					errors.Add("在“{1}”中读取 Xml 节点 {0} 时出现异常：{2}".InvariantFormat(childNode.GetFullPath(), base.Text, ex.Message));
				}
			}
		}

		protected void ReadXmlSettingsGroup(XmlNode xmlNode, ICollection<string> errors)
		{
			SettingsGroup settingsGroup = AllSettingsGroups.Where((SettingsGroup i) => xmlNode.Name == i.XmlNodeName).FirstOrDefault();
			if (settingsGroup != null)
			{
				settingsGroup.ReadXml(xmlNode, this, errors);
				return;
			}
			errors.Add("“{1}”中出现意外的 Xml 节点 {0}".InvariantFormat(xmlNode.GetFullPath(), base.Text));
		}

		internal virtual void UpdateSettings(NodePropertiesDialog dlg)
		{
			if (dlg != null)
			{
				dlg.UpdateSettings();
				if (base.TreeView != null && dlg.PropertiesPage.ParentGroup != null && base.Parent != dlg.PropertiesPage.ParentGroup)
				{
					ServerTree.Instance.MoveNode(this, dlg.PropertiesPage.ParentGroup);
				}
			}
		}

		internal abstract void Show();

		internal abstract void Hide();

		public abstract void Connect();

		public abstract void ConnectAs(LogonCredentials logonSettings, ConnectionSettings connectionSettings);

		public abstract void Reconnect();

		public abstract void Disconnect();

		public abstract void LogOff();

		public abstract void OnRemoving();

		public void DoPropertiesDialog() => DoPropertiesDialog(null, null);

		public abstract void DoPropertiesDialog(Form parentForm, string activeTabName);

		public virtual bool CanRemove(bool popUI) => AllowEdit(popUI);

		public abstract bool ConfirmRemove(bool askUser);

		public abstract bool CanDropOnTarget(RdcTreeNode targetNode);

		public virtual bool HandleMove(RdcTreeNode childNode) => false;

		public virtual void ChangeImageIndex(ImageConstants index)
		{
			base.ImageIndex = (int)index;
			base.SelectedImageIndex = (int)ServerTree.TranslateImage(index, toSelected: true);
		}

		public virtual void CollectNodesToInvalidate(bool recurseChildren, HashSet<RdcTreeNode> set)
		{
			if (recurseChildren)
			{
				foreach (RdcTreeNode node in base.Nodes)
				{
					node.CollectNodesToInvalidate(recurseChildren, set);
				}
			}
			set.Add(this);
		}

		public void ResetInheritance()
		{
			_needToUpdateInheritedSettings = true;
		}

		public virtual void InvalidateNode()
		{
			_needToUpdateInheritedSettings = true;
		}

		public bool InheritSettings()
		{
			bool anyInherited = false;
			if (!_needToUpdateInheritedSettings)
			{
				return anyInherited;
			}
			foreach (SettingsGroup allSettingsGroup in AllSettingsGroups)
			{
				allSettingsGroup.InheritSettings(this, ref anyInherited);
			}
			_needToUpdateInheritedSettings = false;
			return anyInherited;
		}

		public void DoConnectAs()
		{
			RdcTreeNode rdcTreeNode = this;
			if (rdcTreeNode is ServerRef serverRef)
			{
				rdcTreeNode = serverRef.ServerNode;
			}
			using ConnectAsDialog connectAsDialog = ConnectAsDialog.NewConnectAsDialog(rdcTreeNode, Program.TheForm);
			if (connectAsDialog.ShowDialog() == DialogResult.OK)
			{
				connectAsDialog.UpdateSettings();
				ConnectAs(connectAsDialog.LogonCredentials, connectAsDialog.ConnectionSettings);
			}
		}

		public virtual bool AllowEdit(bool popUI)
		{
			GroupBase readOnlyParent = GetReadOnlyParent();
			if (readOnlyParent != null)
			{
				if (popUI)
					FormTools.InformationDialog("{0} “{1}” 是只读的，不能编辑".CultureFormat((readOnlyParent == this) ? "组" : "父组", readOnlyParent.Text));

				return false;
			}
			return true;
		}

		public CredentialsProfile LookupCredentialsProfile(ILogonCredentials logonCredentials)
		{
			CredentialsStore credentialsProfiles = Program.CredentialsProfiles;
			if (logonCredentials.ProfileScope == ProfileScope.File)
			{
				credentialsProfiles = FileGroup.CredentialsProfiles;
			}
			credentialsProfiles.TryGetValue(logonCredentials.ProfileName, out var profile);
			return profile;
		}

		internal void ResolveCredentials()
		{
			ResolveAndFixCredentials(LogonCredentials);
			ResolveAndFixCredentials(GatewaySettings);
		}

		internal bool ResolveCredentials(LogonCredentials logonCredentials)
		{
			if (logonCredentials.ProfileName.Scope == ProfileScope.Local)
			{
				if (!LogonCredentials.IsCustomProfile(logonCredentials.ProfileName.Value))
				{
					logonCredentials.ProfileName.Value = "Custom";
				}
				return true;
			}
			CredentialsProfile credentialsProfile = LookupCredentialsProfile(logonCredentials);
			if (credentialsProfile != null)
			{
				logonCredentials.UserName.Value = credentialsProfile.UserName;
				logonCredentials.Domain.Value = credentialsProfile.Domain;
				if (credentialsProfile.IsDecrypted)
				{
					logonCredentials.Password.SetPlainText(credentialsProfile.Password.Value);
				}
				return true;
			}
			return false;
		}

		private void ResolveAndFixCredentials(LogonCredentials logonCredentials)
		{
			if (logonCredentials != null && !ResolveCredentials(logonCredentials))
			{
				logonCredentials.ProfileName.Reset();
			}
		}
	}
}

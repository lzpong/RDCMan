using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan {
	public class FileGroup : GroupBase {
		internal const string XmlNodeName = "file";

		protected new static Dictionary<string, Helpers.ReadXmlDelegate> NodeActions;

		public override EncryptionSettings EncryptionSettings { get; protected set; }

		internal new int SchemaVersion { get; set; }

		public CredentialsStore CredentialsProfiles { get; private set; }

		public bool HasChangedSinceWrite { get; set; }

		public string Pathname { get; set; }

		static FileGroup() {
			NodeActions = new Dictionary<string, Helpers.ReadXmlDelegate>(GroupBase.NodeActions);
			NodeActions["credentialsProfiles"] = delegate (XmlNode childNode, RdcTreeNode parent, ICollection<string> errors) {
				(parent as FileGroup).CredentialsProfiles.ReadXml(childNode, ProfileScope.File, parent, errors);
			};
			ServerTree.Instance.GroupChanged += OnGroupChanged;
			ServerTree.Instance.ServerChanged += OnServerChanged;
		}

		private static void OnServerChanged(ServerChangedEventArgs e) {
			if (e.ChangeType.HasFlag(ChangeType.TreeChanged) || e.ChangeType.HasFlag(ChangeType.PropertyChanged)) {
				FileGroup fileGroup = e.Server.FileGroup;
				if (fileGroup != null)
					fileGroup.HasChangedSinceWrite = true;
			}
		}

		private static void OnGroupChanged(GroupChangedEventArgs e) {
			if (e.ChangeType.HasFlag(ChangeType.TreeChanged) || e.ChangeType.HasFlag(ChangeType.PropertyChanged)) {
				FileGroup fileGroup = e.Group.FileGroup;
				if (fileGroup != null)
					fileGroup.HasChangedSinceWrite = true;
			}
		}

		internal FileGroup(string pathname) {
			Pathname = Path.GetFullPath(pathname);
			if (File.Exists(Pathname)) {
				base.IsReadOnly = File.GetAttributes(Pathname).HasFlag(FileAttributes.ReadOnly);
			}
			else {
				base.Properties.GroupName.Value = Path.GetFileNameWithoutExtension(Pathname);
				base.Text = base.Properties.GroupName.Value;
			}
			base.ToolTipText = pathname;
			ChangeImageIndex(ImageConstants.File);
			CredentialsProfiles = new CredentialsStore();
			EncryptionSettings = new EncryptionSettings();
			base.AllSettingsGroups.Add(EncryptionSettings);
		}

		public string GetFilename() {
			return Path.GetFileName(Pathname);
		}

		public string GetDirectory() {
			return Path.GetDirectoryName(Pathname);
		}

		protected override void InitSettings() {
			((RdcTreeNode)this).Properties = new FileGroupSettings();
			base.InitSettings();
		}

		internal override void ReadXml(XmlNode xmlNode, ICollection<string> errors) {
			ReadXml(NodeActions, xmlNode, errors);
			base.Text = base.Properties.GroupName.Value;
			if (base.IsReadOnly) {
				base.Text += " {RO}";
			}
		}

		internal override void WriteXml(XmlTextWriter tw) {
			tw.WriteStartElement("file");
			CredentialsProfiles.WriteXml(tw, this);
			base.WriteXml(tw);
			tw.WriteEndElement();
		}

		public sealed override bool ConfirmRemove(bool askUser) {
			FormTools.InformationDialog("ʹ���ļ��˵��ر� " + base.Text + " ��");
			return false;
		}

		public override void DoPropertiesDialog(Form parentForm, string activeTabName) {
			using FileGroupPropertiesDialog fileGroupPropertiesDialog = FileGroupPropertiesDialog.NewPropertiesDialog(this, parentForm);
			fileGroupPropertiesDialog.SetActiveTab(activeTabName);
			if (fileGroupPropertiesDialog.ShowDialog() == DialogResult.OK) {
				UpdateSettings(fileGroupPropertiesDialog);
				ServerTree.Instance.OnNodeChanged(this, ChangeType.PropertyChanged);
			}
		}

		internal void CheckCredentials() {
			Dictionary<string, List<string>> missingProfiles = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			this.VisitNodes(delegate (RdcTreeNode node) {
				CheckCredentials(node, node.LogonCredentials, "��¼ƾ֤", missingProfiles);
				CheckCredentials(node, node.GatewaySettings, "��������", missingProfiles);
			});
			if (missingProfiles.Count <= 0) {
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("δ�ҵ�ĳЩƾ֤�����ļ���")
				.AppendLine("�������ӵ����������༭���Ե�֮ǰ���ȱ�ٵ������ļ�������ο�����ʧ��")
				.AppendLine("���� ȷ�� ����ϸ��Ϣ���Ƶ������塣")
				.AppendLine();
			foreach (KeyValuePair<string, List<string>> item in missingProfiles) {
				stringBuilder.AppendLine("�����ļ����ƣ�" + item.Key);
				stringBuilder.AppendFormat("�����ڣ� ");
				foreach (string item2 in item.Value) {
					stringBuilder.Append(" " + item2);
				}
				stringBuilder.AppendLine().AppendLine();
			}
			DialogResult dialogResult = FormTools.ExclamationDialog(stringBuilder.ToString(), MessageBoxButtons.OKCancel);
			if (dialogResult == DialogResult.OK) {
				Clipboard.SetText(stringBuilder.ToString());
			}
		}

		private void CheckCredentials(RdcTreeNode node, LogonCredentials credentials, string name, Dictionary<string, List<string>> missingProfiles) {
			if (credentials != null && credentials.InheritSettingsType.Mode != 0 && !node.ResolveCredentials(credentials)) {
				string key = LogonCredentials.ConstructQualifiedName(credentials);
				if (!missingProfiles.TryGetValue(key, out var value)) {
					List<string> list2 = (missingProfiles[key] = new List<string>());
					value = list2;
				}
				value.Add($"{node.FullPath}.{name}");
			}
		}
	}
}

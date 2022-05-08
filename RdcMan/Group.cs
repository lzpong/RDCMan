using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan {
	public class Group : GroupBase {
		//public const string XmlNodeName = "group";

		protected Group() { }

		internal static Group CreateForAddDialog() {
			return new Group();
		}

		public static Group Create(string name, GroupBase parent) {
			Group group = new Group();
			group.Properties.GroupName.Value = name;
			group.FinishConstruction(parent);
			return group;
		}

		internal static Group Create(GroupPropertiesDialog dlg) {
			Group group = dlg.AssociatedNode as Group;
			group.UpdateSettings(dlg);
			group.FinishConstruction(dlg.PropertiesPage.ParentGroup);
			return group;
		}

		internal static Group Create(XmlNode xmlNode, GroupBase parent, ICollection<string> errors) {
			Group group = new Group();
			group.ReadXml(xmlNode, errors);
			group.FinishConstruction(parent);
			return group;
		}

		protected override void InitSettings() {
			((RdcTreeNode)this).Properties = new GroupSettings();
			base.InitSettings();
		}

		internal override void WriteXml(XmlTextWriter tw) {
			tw.WriteStartElement("group");
			base.WriteXml(tw);
			tw.WriteEndElement();
		}

		public override void DoPropertiesDialog(Form parentForm, string activeTabName) {
			using GroupPropertiesDialog groupPropertiesDialog = GroupPropertiesDialog.NewPropertiesDialog(this, parentForm);
			groupPropertiesDialog.SetActiveTab(activeTabName);
			if (groupPropertiesDialog.ShowDialog() == DialogResult.OK) {
				UpdateSettings(groupPropertiesDialog);
				ServerTree.Instance.OnNodeChanged(this, ChangeType.PropertyChanged);
			}
		}

		private void FinishConstruction(GroupBase parent) {
			base.Text = base.Properties.GroupName.Value;
			ServerTree.Instance.AddNode(this, parent);
			ChangeImageIndex(ImageConstants.Group);
		}
	}
}

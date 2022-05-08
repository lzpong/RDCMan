using System;
using System.Windows.Forms;

namespace RdcMan {
	public abstract class NodePropertiesPage<TSettingsGroup> : SettingsTabPage<TSettingsGroup>, INodePropertiesPage where TSettingsGroup : SettingsGroup {
		protected ValueComboBox<GroupBase> _parentComboBox;

		public GroupBase ParentGroup {
			get {
				return _parentComboBox == null || !_parentComboBox.Enabled ? null : _parentComboBox.SelectedValue;
			}
		}

		public event Action<GroupBase> ParentGroupChanged;

		protected NodePropertiesPage(TabbedSettingsDialog dialog, TSettingsGroup settings, string name)
			: base(dialog, settings, name) { }

		public bool PopulateParentDropDown(GroupBase excludeGroup, GroupBase defaultParent) {
			PopulateParentDropDown(excludeGroup);
			if (defaultParent != null && CanBeParent(defaultParent))
				_parentComboBox.SelectedValue = defaultParent;
			else {
				if (_parentComboBox.ItemCount == 0)
					return false;

				_parentComboBox.SelectedIndex = 0;
			}
			return true;
		}

		public void SetParentDropDown(GroupBase group) {
			_parentComboBox.AddItem(group.FullPath, group);
			_parentComboBox.SelectedIndex = 0;
		}

		protected abstract bool CanBeParent(GroupBase group);

		private void PopulateParentDropDown(GroupBase excludeGroup) {
			ServerTree.Instance.Nodes.VisitNodes(delegate (RdcTreeNode node) {
				if (node == excludeGroup)
					return NodeVisitorResult.NoRecurse;
				if (node is GroupBase groupBase && CanBeParent(groupBase))
					_parentComboBox.AddItem(groupBase.FullPath, groupBase);
				return NodeVisitorResult.Continue;
			});
		}

		protected void AddParentCombo(ref int rowIndex, ref int tabIndex) {
			_parentComboBox = FormTools.AddLabeledValueDropDown<GroupBase>(this, "¸¸¼¶(&P)£º", ref rowIndex, ref tabIndex, null, null);
			_parentComboBox.SelectedIndexChanged += ParentGroupChangedHandler;
		}

		protected RdcTextBox AddComment(ref int rowIndex, ref int tabIndex) {
			Label label = FormTools.NewLabel("±¸×¢(&C)£º", 0, rowIndex);
			RdcTextBox rdcTextBox = FormTools.NewTextBox(1, rowIndex++, tabIndex++, 7);
			rdcTextBox.Enabled = true;
			rdcTextBox.Height += 46;
			base.Controls.Add(label, rdcTextBox);
			return rdcTextBox;
		}

		protected virtual void ParentGroupChangedHandler(object sender, EventArgs e) {
			GroupBase selectedValue = _parentComboBox.SelectedValue;
			this.ParentGroupChanged(selectedValue);
		}
	}
}

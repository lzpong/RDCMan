using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan {
	internal abstract class BuiltInVirtualGroup<TServerRef> : VirtualGroup, IBuiltInVirtualGroup where TServerRef : ServerRef {
		protected new static Dictionary<string, Helpers.ReadXmlDelegate> NodeActions;

		string IBuiltInVirtualGroup.Text => base.Text;

		string IBuiltInVirtualGroup.ConfigPropertyName => ConfigName;

		string IBuiltInVirtualGroup.XmlNodeName => XmlNodeName;

		bool IBuiltInVirtualGroup.IsInTree {
			get => base.TreeView != null;
			set => UpdateVisibleInTree(value);
		}

		bool IBuiltInVirtualGroup.IsVisibilityConfigurable => IsVisibilityConfigurable;

		public virtual string ConfigName => XmlNodeName.Substring(0, 1).ToUpper() + XmlNodeName.Substring(1);

		public bool IsInTree {
			get => base.TreeView != null;
			set => UpdateVisibleInTree(value);
		}

		public override bool HasProperties => false;

		protected virtual string XmlNodeName => null;

		protected virtual bool IsVisibilityConfigurable => true;

		void IBuiltInVirtualGroup.ReadXml(XmlNode xmlNode, FileGroup fileGroup, ICollection<string> errors) {
			ReadXml(xmlNode, fileGroup, errors);
		}

		void IBuiltInVirtualGroup.WriteXml(XmlTextWriter tw, FileGroup fileGroup) {
			WriteXml(tw, fileGroup);
		}

		bool IBuiltInVirtualGroup.ShouldWriteNode(ServerRef serverRef, FileGroup file) {
			return ShouldWriteNode(serverRef, file);
		}

		static BuiltInVirtualGroup() {
			NodeActions = new Dictionary<string, Helpers.ReadXmlDelegate>(GroupBase.NodeActions);
			NodeActions["server"] = delegate (XmlNode childNode, RdcTreeNode parent, ICollection<string> errors) {
				TreeNode treeNode = ServerTree.Instance.FindNodeByName(childNode.InnerText);
				if (treeNode != null && treeNode is Server serverBase)
					(parent as BuiltInVirtualGroup<TServerRef>).AddReference(serverBase);
			};
		}

		public override void OnRemoving() {
			Hide();
		}

		public sealed override bool ConfirmRemove(bool askUser) {
			FormTools.InformationDialog("Use the View menu to hide the " + base.Text + " group");
			return false;
		}

		public sealed override bool CanDropOnTarget(RdcTreeNode targetNode) {
			return false;
		}

		public override bool CanDropServers() {
			return false;
		}

		public sealed override bool CanRemove(bool popUI) {
			return false;
		}

		public override void DoPropertiesDialog(Form parentForm, string activeTabName) {
			throw new NotImplementedException();
		}

		internal override void ReadXml(XmlNode xmlNode, ICollection<string> errors) {
			throw new NotImplementedException();
		}

		internal override void WriteXml(XmlTextWriter tw) {
			throw new NotImplementedException();
		}

		protected virtual void ReadXml(XmlNode xmlNode, FileGroup fileGroup, ICollection<string> errors) {
			if (!string.IsNullOrEmpty(XmlNodeName)) {
				ReadXml(NodeActions, xmlNode, errors);
				if (base.Properties.Expanded.Value)
					Expand();
			}
		}

		protected virtual void WriteXml(XmlTextWriter tw, FileGroup file) {
			if (string.IsNullOrEmpty(XmlNodeName))
				return;

			tw.WriteStartElement(XmlNodeName);
			if (file == null)
				WriteXmlSettingsGroups(tw);

			foreach (TreeNode node in base.Nodes) {
				TServerRef val = node as TServerRef;
				if (ShouldWriteNode(val, file))
					tw.WriteElementString("server", val.ServerNode.FullPath);
			}
			tw.WriteEndElement();
		}

		protected virtual bool ShouldWriteNode(RdcTreeNode node, FileGroup file) {
			return node.FileGroup == file;
		}

		public virtual TServerRef AddReference(ServerBase serverBase) {
			if (serverBase == null)
				return null;

			Server serverNode = serverBase.ServerNode;
			TServerRef val = serverNode.FindServerRef<TServerRef>();
			if (val == null) {
				val = base.ServerRefFactory.Create(serverNode) as TServerRef;
				ServerTree.Instance.AddNode(val, this);
			}
			return val;
		}

		protected void UpdateVisibleInTree(bool isVisible) {
			if (isVisible) {
				if (base.TreeView == null) {
					ServerTree.Instance.AddNode(this, ServerTree.Instance.RootNode);
					Expand();
					ServerTree.Instance.Operation(OperationBehavior.RestoreSelected, delegate {
						ServerTree.Instance.SortBuiltinGroups();
					});
				}
			}
			else if (base.TreeView != null)
				ServerTree.Instance.RemoveNode(this);
			if (IsVisibilityConfigurable)
				Program.Preferences.SetBuiltInGroupVisibility(this, isVisible);
		}
	}
}

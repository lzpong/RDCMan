using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace RdcMan {
	internal class SmartGroup : VirtualGroup {
		internal const string XmlNodeName = "smartGroup";

		private static readonly List<SmartGroup> AllSmartGroups;

		protected new static Dictionary<string, Helpers.ReadXmlDelegate> NodeActions;

		public new SmartGroupSettings Properties => base.Properties as SmartGroupSettings;

		public RuleGroup RuleGroup { get; private set; }

		static SmartGroup() {
			AllSmartGroups = new List<SmartGroup>();
			NodeActions = new Dictionary<string, Helpers.ReadXmlDelegate>(GroupBase.NodeActions);
			NodeActions["ruleGroup"] = delegate (XmlNode childNode, RdcTreeNode node, ICollection<string> errors) {
				(node as SmartGroup).RuleGroup = RuleGroup.Create(childNode, node, errors);
			};
			ServerTree.Instance.GroupChanged += OnGroupChanged;
			ServerTree.Instance.ServerChanged += OnServerChanged;
		}

		protected SmartGroup() {
			AllSmartGroups.Add(this);
		}

		protected override void InitSettings() {
			((RdcTreeNode)this).Properties = new SmartGroupSettings();
			base.AllSettingsGroups.Add(Properties);
			RuleGroup = new RuleGroup(RuleGroupOperator.All, Enumerable.Empty<Rule>());
		}

		public static SmartGroup CreateForAdd() {
			return new SmartGroup();
		}

		public static SmartGroup Create(SmartGroupPropertiesDialog dlg) {
			SmartGroup smartGroup = dlg.AssociatedNode as SmartGroup;
			smartGroup.UpdateSettings(dlg);
			smartGroup.FinishConstruction(dlg.PropertiesPage.ParentGroup);
			smartGroup.Refresh();
			return smartGroup;
		}

		public static SmartGroup Create(XmlNode xmlNode, GroupBase parent, ICollection<string> errors) {
			SmartGroup smartGroup = new SmartGroup();
			smartGroup.FinishConstruction(parent);
			smartGroup.ReadXml(xmlNode, errors);
			smartGroup.Text = smartGroup.Properties.GroupName.Value;
			return smartGroup;
		}

		public sealed override bool CanDropServers() {
			return false;
		}

		public override void OnRemoving() {
			base.OnRemoving();
			AllSmartGroups.Remove(this);
		}

		public static void OnGroupChanged(GroupChangedEventArgs e) {
			if (e.ChangeType.HasFlag(ChangeType.TreeChanged) || e.ChangeType.HasFlag(ChangeType.PropertyChanged)) {
				RefreshScope(e.Group, delegate (SmartGroup group) {
					group.Refresh();
				});
			}
		}

		public static void OnServerChanged(ServerChangedEventArgs e) {
			if (!e.ChangeType.HasFlag(ChangeType.TreeChanged) && !e.ChangeType.HasFlag(ChangeType.PropertyChanged))
				return;

			Server server = e.Server as Server;
			if (server != null) {
				bool dummy = false;
				RefreshScope(server.Parent as GroupBase, delegate (SmartGroup group) {
					group.UpdateForServer(server, ref dummy);
				});
			}
		}

		public static void RefreshAll(FileGroup fileGroup) {
			AllSmartGroups.ForEach(delegate (SmartGroup group) {
				if (group.FileGroup == fileGroup)
					group.Refresh();
			});
		}

		private static void RefreshScope(GroupBase scope, Action<SmartGroup> process) {
			scope?.VisitParents(delegate (RdcTreeNode parent) {
				foreach (SmartGroup item in parent.Nodes.OfType<SmartGroup>()) {
					process(item);
				}
			});
		}

		public sealed override void DoPropertiesDialog(Form parentForm, string activeTabName) {
			using SmartGroupPropertiesDialog smartGroupPropertiesDialog = SmartGroupPropertiesDialog.NewPropertiesDialog(this, parentForm);
			smartGroupPropertiesDialog.SetActiveTab(activeTabName);
			if (smartGroupPropertiesDialog.ShowDialog() == DialogResult.OK) {
				UpdateSettings(smartGroupPropertiesDialog);
				Refresh();
			}
		}

		public sealed override bool CanRemoveChildren() {
			return false;
		}

		internal override void ReadXml(XmlNode xmlNode, ICollection<string> errors) {
			ReadXml(NodeActions, xmlNode, errors);
		}

		internal override void WriteXml(XmlTextWriter tw) {
			tw.WriteStartElement("smartGroup");
			Properties.Expanded.Value = base.IsExpanded;
			WriteXmlSettingsGroups(tw);
			RuleGroup.WriteXml(tw);
			tw.WriteEndElement();
		}

		public void Refresh() {
			bool changed = false;
			using (Helpers.Timer("refreshing smart group {0}", base.Text)) {
				ServerTree.Instance.Operation(OperationBehavior.SuspendSort | OperationBehavior.SuspendUpdate | OperationBehavior.SuspendGroupChanged, delegate {
					HashSet<SmartServerRef> set = new HashSet<SmartServerRef>();
					base.Nodes.ForEach(delegate (TreeNode s) {
						set.Add(s as SmartServerRef);
					});
					this.GetParentNodes().VisitNodes(delegate (RdcTreeNode node) {
						if (node is VirtualGroup)
							return NodeVisitorResult.NoRecurse;
						if (node is Server server) {
							SmartServerRef item = UpdateForServer(server, ref changed);
							set.Remove(item);
						}
						return NodeVisitorResult.Continue;
					});
					if (set.Count > 0) {
						changed = true;
						set.ForEach(delegate (SmartServerRef s) {
							ServerTree.Instance.RemoveNode(s);
						});
					}
				});
			}
			if (changed) {
				ServerTree.Instance.SortGroup(this);
				ServerTree.Instance.OnGroupChanged(this, ChangeType.InvalidateUI);
			}
		}

		private SmartServerRef UpdateForServer(Server server, ref bool changed) {
			SmartServerRef smartServerRef = server.FindServerRef<SmartServerRef>(this);
			bool flag = RuleGroup != null && RuleGroup.Evaluate(server);
			if (smartServerRef != null != flag) {
				changed = true;
				if (flag) {
					smartServerRef = new SmartServerRef(server);
					ServerTree.Instance.AddNode(smartServerRef, this);
				}
				else
					ServerTree.Instance.RemoveNode(smartServerRef);
			}
			return smartServerRef;
		}

		private void FinishConstruction(GroupBase parent) {
			ServerTree.Instance.AddNode(this, parent);
			ChangeImageIndex(ImageConstants.SmartGroup);
		}
	}
}

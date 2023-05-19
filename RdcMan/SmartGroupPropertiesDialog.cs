using System.Windows.Forms;

namespace RdcMan
{
	internal class SmartGroupPropertiesDialog : NodePropertiesDialog
	{
		protected SmartGroupPropertiesDialog(SmartGroup group, string dialogTitle, string acceptButtonText, Form parentForm)
			: base(group, dialogTitle, acceptButtonText, parentForm)
		{
		}

		public override void CreateControls(RdcTreeNode settings)
		{
			TabPage tabPage = settings.Properties.CreateTabPage(this);
			base.PropertiesPage = tabPage as INodePropertiesPage;
			AddTabPage(tabPage);
			InitButtons();
			this.ScaleAndLayout();
		}

		public static SmartGroupPropertiesDialog NewAddDialog(GroupBase parent)
		{
			SmartGroup smartGroup = SmartGroup.CreateForAdd();
			SmartGroupPropertiesDialog smartGroupPropertiesDialog = new SmartGroupPropertiesDialog(smartGroup, "添加智能组", "添加", null);
			if (parent != null && !parent.CanAddGroups())
			{
				parent = null;
			}
			smartGroupPropertiesDialog.CreateControls(smartGroup);
			if (!smartGroupPropertiesDialog.PropertiesPage.PopulateParentDropDown(null, parent))
			{
				smartGroupPropertiesDialog.Dispose();
				return null;
			}
			return smartGroupPropertiesDialog;
		}

		public static SmartGroupPropertiesDialog NewPropertiesDialog(SmartGroup group, Form parentForm)
		{
			SmartGroupPropertiesDialog smartGroupPropertiesDialog = new SmartGroupPropertiesDialog(group, group.Text + " 智能组属性", "确定", parentForm);
			smartGroupPropertiesDialog.CreateControls(group);
			smartGroupPropertiesDialog.PropertiesPage.PopulateParentDropDown(group, group.Parent as GroupBase);
			return smartGroupPropertiesDialog;
		}
	}
}

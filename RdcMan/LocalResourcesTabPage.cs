using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MSTSCLib;

namespace RdcMan
{
	public class LocalResourcesTabPage : SettingsTabPage<LocalResourcesSettings>
	{
		private bool _processingAfterCheck;

		private TreeNode _redirectDrivesCheckBox;

		private TreeNode _redirectPrintersCheckBox;

		private TreeNode _redirectPortsCheckBox;

		private TreeNode _redirectSmartCardsCheckBox;

		private TreeNode _redirectClipboardCheckBox;

		private TreeNode _redirectPnpDevicesCheckBox;

		public LocalResourcesTabPage(TabbedSettingsDialog dialog, LocalResourcesSettings settings)
			: base(dialog, settings)
		{
			int num = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref num);
			FormTools.AddLabeledEnumDropDown(this, "远端声音(&S)：", base.Settings.AudioRedirectionMode, ref rowIndex, ref num, RdpClient.AudioRedirectionModeToString);
			if (RdpClient.SupportsAdvancedAudioVideoRedirection) {
				FormTools.AddLabeledEnumDropDown(this, "声音质量(&Q)：", base.Settings.AudioRedirectionQuality, ref rowIndex, ref num, RdpClient.AudioRedirectionQualityToString);
				FormTools.AddLabeledEnumDropDown(this, "远端录音(&R)：", base.Settings.AudioCaptureRedirectionMode, ref rowIndex, ref num, RdpClient.AudioCaptureRedirectionModeToString);
			}
			FormTools.AddLabeledEnumDropDown(this, "组合键(&W)：", base.Settings.KeyboardHookMode, ref rowIndex, ref num, RdpClient.KeyboardHookModeToString);
			Label value = FormTools.NewLabel("设备重定向(&D)：", 0, rowIndex);
			TreeView treeView = new TreeView {
				Location = FormTools.NewLocation(1, rowIndex),
				Size = new Size(340, 160),
				CheckBoxes = true,
				Scrollable = true,
				ShowLines = false
			};
			treeView.AfterCheck += RedirectView_AfterCheck;
			_redirectClipboardCheckBox = treeView.Nodes.Add("剪贴板");
			_redirectPrintersCheckBox = treeView.Nodes.Add("打印机");
			_redirectSmartCardsCheckBox = treeView.Nodes.Add("智能卡");
			_redirectPortsCheckBox = treeView.Nodes.Add("串口端口");
			_redirectDrivesCheckBox = treeView.Nodes.Add("Drives");
			_redirectPnpDevicesCheckBox = treeView.Nodes.Add("PnP设备");
			if (RdpClient.SupportsFineGrainedRedirection)
			{
				IMsRdpDriveCollection driveCollection = RdpClient.DriveCollection;
				for (uint num2 = 0u; num2 < driveCollection.DriveCount; num2++)
				{
					IMsRdpDrive msRdpDrive = driveCollection.get_DriveByIndex(num2);
					_redirectDrivesCheckBox.Nodes.Add(msRdpDrive.Name.Substring(0, msRdpDrive.Name.Length - 1));
				}
			}
			base.Controls.Add(value);
			base.Controls.Add(treeView);
		}

		protected override void UpdateControls()
		{
			base.UpdateControls();
			_redirectDrivesCheckBox.Checked = base.Settings.RedirectDrives.Value;
			_redirectPortsCheckBox.Checked = base.Settings.RedirectPorts.Value;
			_redirectPrintersCheckBox.Checked = base.Settings.RedirectPrinters.Value;
			_redirectSmartCardsCheckBox.Checked = base.Settings.RedirectSmartCards.Value;
			_redirectClipboardCheckBox.Checked = base.Settings.RedirectClipboard.Value;
			_redirectPnpDevicesCheckBox.Checked = base.Settings.RedirectPnpDevices.Value;
			foreach (string item in base.Settings.RedirectDrivesList.Value) {
				foreach (TreeNode node in _redirectDrivesCheckBox.Nodes) {
					if (node.Text == item) {
						_redirectDrivesCheckBox.Expand();
						node.Checked = true;
					}
				}
			}
		}

		protected override void UpdateSettings()
		{
			base.UpdateSettings();
			base.Settings.RedirectDrives.Value = _redirectDrivesCheckBox.Checked;
			List<string> list = new List<string>();
			foreach (TreeNode node in _redirectDrivesCheckBox.Nodes) {
				if (node.Checked)
					list.Add(node.Text);
			}
			base.Settings.RedirectDrivesList.Value = list;
			base.Settings.RedirectPorts.Value = _redirectPortsCheckBox.Checked;
			base.Settings.RedirectPrinters.Value = _redirectPrintersCheckBox.Checked;
			base.Settings.RedirectSmartCards.Value = _redirectSmartCardsCheckBox.Checked;
			base.Settings.RedirectClipboard.Value = _redirectClipboardCheckBox.Checked;
			base.Settings.RedirectPnpDevices.Value = _redirectPnpDevicesCheckBox.Checked;
		}

		private void RedirectView_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (_processingAfterCheck)
			{
				return;
			}
			_processingAfterCheck = true;
			if (e.Node.Nodes.Count == 0 && e.Node.Parent != null)
			{
				e.Node.Parent.Checked = e.Node.Parent.Nodes.Cast<TreeNode>().All((TreeNode node) => node.Checked);
			}
			else
			{
				foreach (TreeNode node in e.Node.Nodes)
					node.Checked = e.Node.Checked;
			}
			_processingAfterCheck = false;
		}
	}
}

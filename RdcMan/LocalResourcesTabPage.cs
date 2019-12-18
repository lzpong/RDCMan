using MSTSCLib;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
			int tabIndex = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref tabIndex);
			FormTools.AddLabeledEnumDropDown(this, "Remote &sound", base.Settings.AudioRedirectionMode, ref rowIndex, ref tabIndex, RdpClient.AudioRedirectionModeToString);
			if (RdpClient.SupportsAdvancedAudioVideoRedirection)
			{
				FormTools.AddLabeledEnumDropDown(this, "Sound &quality", base.Settings.AudioRedirectionQuality, ref rowIndex, ref tabIndex, RdpClient.AudioRedirectionQualityToString);
				FormTools.AddLabeledEnumDropDown(this, "Remote &recording", base.Settings.AudioCaptureRedirectionMode, ref rowIndex, ref tabIndex, RdpClient.AudioCaptureRedirectionModeToString);
			}
			FormTools.AddLabeledEnumDropDown(this, "&Windows key combos", base.Settings.KeyboardHookMode, ref rowIndex, ref tabIndex, RdpClient.KeyboardHookModeToString);
			Label value = FormTools.NewLabel("Redirect options", 0, rowIndex);
			TreeView treeView = new TreeView
			{
				Location = FormTools.NewLocation(1, rowIndex),
				Size = new Size(340, 140),
				CheckBoxes = true,
				Scrollable = true,
				ShowLines = false
			};
			treeView.AfterCheck += RedirectView_AfterCheck;
			_redirectClipboardCheckBox = treeView.Nodes.Add("Clipboard");
			_redirectPrintersCheckBox = treeView.Nodes.Add("Printers");
			_redirectSmartCardsCheckBox = treeView.Nodes.Add("Smart cards");
			_redirectPortsCheckBox = treeView.Nodes.Add("Ports");
			_redirectDrivesCheckBox = treeView.Nodes.Add("Drives");
			_redirectPnpDevicesCheckBox = treeView.Nodes.Add("PnP devices");
			if (RdpClient.SupportsFineGrainedRedirection)
			{
				IMsRdpDriveCollection driveCollection = RdpClient.DriveCollection;
				for (uint num = 0u; num < driveCollection.DriveCount; num++)
				{
					IMsRdpDrive msRdpDrive = driveCollection.get_DriveByIndex(num);
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
			foreach (string item in base.Settings.RedirectDrivesList.Value)
			{
				foreach (TreeNode node in _redirectDrivesCheckBox.Nodes)
				{
					if (node.Text == item)
					{
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
			foreach (TreeNode node in _redirectDrivesCheckBox.Nodes)
			{
				if (node.Checked)
				{
					list.Add(node.Text);
				}
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
			if (!_processingAfterCheck)
			{
				_processingAfterCheck = true;
				if (e.Node.Nodes.Count == 0 && e.Node.Parent != null)
				{
					e.Node.Parent.Checked = e.Node.Parent.Nodes.Cast<TreeNode>().All((TreeNode node) => node.Checked);
				}
				else
				{
					foreach (TreeNode node in e.Node.Nodes)
					{
						node.Checked = e.Node.Checked;
					}
				}
				_processingAfterCheck = false;
			}
		}
	}
}

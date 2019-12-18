using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan
{
	public class TabbedSettingsDialog : RdcDialog
	{
		private TabPage _initiallyActiveTab;

		private readonly TabControl _tabControl;

		public IEnumerable<TabPage> TabPages => _tabControl.TabPages.Cast<TabPage>();

		public TabbedSettingsDialog(string dialogTitle, string acceptButtonText, Form parentForm)
			: base(dialogTitle, acceptButtonText, parentForm)
		{
			_tabControl = new TabControl
			{
				Location = new Point(8, 8),
				SelectedIndex = 0,
				Size = new Size(520, 350),
				Appearance = TabAppearance.Normal,
				Multiline = true
			};
			base.Controls.Add(_tabControl);
		}

		public void AddTabPage(TabPage page)
		{
			_tabControl.TabPages.Add(page);
		}

		public void SetActiveTab(string name)
		{
			_initiallyActiveTab = TabPages.Where((TabPage p) => p.Text == name).FirstOrDefault();
		}

		public void UpdateSettings()
		{
			foreach (ISettingsTabPage item in TabPages.OfType<ISettingsTabPage>())
			{
				item.UpdateSettings();
			}
		}

		public void EnableTabs(EnableTabsEventArgs args)
		{
			foreach (string name in args.TabNames)
			{
				IEnumerable<TabPage> tabPages = TabPages;
				Func<TabPage, bool> predicate = (TabPage p) => p.Text.Equals(name);
				ISettingsTabPage settingsTabPage = tabPages.Where(predicate).First() as ISettingsTabPage;
				settingsTabPage.InheritanceControl.Enable(args.Enabled, args.Reason);
			}
		}

		protected override void ShownCallback(object sender, EventArgs args)
		{
			foreach (TabPage tabPage in TabPages)
			{
				tabPage.Enabled = _acceptButton.Enabled;
				ISettingsTabPage settingsTabPage = tabPage as ISettingsTabPage;
				if (settingsTabPage != null)
				{
					settingsTabPage.UpdateControls();
					if (settingsTabPage.FocusControl != null)
					{
						_tabControl.SelectedTab = tabPage;
						settingsTabPage.FocusControl.Focus();
					}
				}
			}
			if (_initiallyActiveTab != null)
			{
				_tabControl.SelectedTab = _initiallyActiveTab;
			}
		}

		protected override void AcceptIfValid(object sender, EventArgs e)
		{
			TabPage tabPage = null;
			foreach (TabPage tabPage2 in TabPages)
			{
				ISettingsTabPage settingsTabPage = tabPage2 as ISettingsTabPage;
				if (settingsTabPage != null && !settingsTabPage.Validate() && (tabPage == null || tabPage2 == _tabControl.SelectedTab))
				{
					tabPage = tabPage2;
				}
			}
			if (tabPage == null)
			{
				base.AcceptIfValid(sender, e);
			}
			else
			{
				_tabControl.SelectedTab = tabPage;
			}
		}
	}
}

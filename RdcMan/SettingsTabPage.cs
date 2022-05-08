using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan {
	public class SettingsTabPage : TabPage, ISettingsTabPage {
		private const string InvalidSuffix = " (!)";

		InheritanceControl ISettingsTabPage.InheritanceControl {
			get {
				throw new NotImplementedException();
			}
		}

		Control ISettingsTabPage.FocusControl => FocusControl;

		protected Control FocusControl { get; set; }

		void ISettingsTabPage.UpdateControls() {
			UpdateControls();
		}

		bool ISettingsTabPage.Validate() {
			string text = Text.Replace(" (!)", string.Empty);
			if (IsValid()) {
				Text = text;
				return true;
			}
			Text = text + " (!)";
			return false;
		}

		void ISettingsTabPage.UpdateSettings() {
			UpdateSettings();
		}

		protected virtual void UpdateControls() {
			foreach (ISettingControl item in base.Controls.FlattenControls().OfType<ISettingControl>()) {
				item.UpdateControl();
			}
		}

		protected virtual bool IsValid() {
			RdcDialog rdcDialog = FindForm() as RdcDialog;
			return rdcDialog.ValidateControls(base.Controls.FlattenControls(), isValid: true);
		}

		protected virtual void UpdateSettings() {
			foreach (ISettingControl item in base.Controls.FlattenControls().OfType<ISettingControl>()) {
				item.UpdateSetting();
			}
		}
	}
	public abstract class SettingsTabPage<TSettingsGroup> : SettingsTabPage, ISettingsTabPage where TSettingsGroup : SettingsGroup {
		protected InheritanceControl InheritanceControl { get; private set; }

		protected TSettingsGroup Settings { get; private set; }

		protected TabbedSettingsDialog Dialog { get; private set; }

		InheritanceControl ISettingsTabPage.InheritanceControl => InheritanceControl;

		protected SettingsTabPage(TabbedSettingsDialog dialog, TSettingsGroup settingsGroup)
			: this(dialog, settingsGroup, settingsGroup.Name) { }

		protected SettingsTabPage(TabbedSettingsDialog dialog, TSettingsGroup settingsGroup, string name) {
			Dialog = dialog;
			Settings = settingsGroup;
			base.Location = FormTools.TopLeftLocation();
			base.Size = new Size(512, 334);
			Text = name;
		}

		protected void CreateInheritanceControl(ref int rowIndex, ref int tabIndex) {
			if (Settings.InheritSettingsType.Mode != InheritanceMode.Disabled) {
				InheritanceControl = new InheritanceControl(Dialog, Settings.Name);
				InheritanceControl.Create(this, ref rowIndex, ref tabIndex);
			}
		}

		void ISettingsTabPage.UpdateControls() {
			UpdateControls();
		}

		void ISettingsTabPage.UpdateSettings() {
			UpdateSettings();
		}

		protected override void UpdateControls() {
			if (InheritanceControl != null)
				InheritanceControl.UpdateControlsFromSettings(Settings.InheritSettingsType);
			else {
				foreach (Control item in base.Controls.FlattenControls()) {
					item.Enabled = true;
				}
			}
			base.UpdateControls();
		}

		protected override void UpdateSettings() {
			if (InheritanceControl != null)
				Settings.InheritSettingsType.Mode = InheritanceControl.GetInheritanceMode();

			base.UpdateSettings();
		}
	}
}

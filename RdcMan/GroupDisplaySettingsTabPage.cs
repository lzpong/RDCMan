using System.Drawing;

namespace RdcMan {
	public class GroupDisplaySettingsTabPage : DisplaySettingsTabPage<GroupDisplaySettings> {
		private readonly RdcCheckBox _previewCheckBox;

		private readonly RdcCheckBox _interactionCheckBox;

		public GroupDisplaySettingsTabPage(TabbedSettingsDialog dialog, GroupDisplaySettings settings)
			: base(dialog, settings) {
			Create(out var rowIndex, out var num);
			_previewCheckBox = FormTools.AddCheckBox(this, "在缩略图中预览会话(&P)", settings.SessionThumbnailPreview, 0, ref rowIndex, ref num);
			_interactionCheckBox = FormTools.AddCheckBox(this, "允许缩略图会话交互(&A)", settings.AllowThumbnailSessionInteraction, 0, ref rowIndex, ref num);
			_interactionCheckBox.Location = new Point(_previewCheckBox.Left + 24, _interactionCheckBox.Top);
			_previewCheckBox.CheckedChanged += delegate {
				PreviewCheckBoxChanged();
			};
			if (base.InheritanceControl != null) {
				base.InheritanceControl.EnabledChanged += delegate {
					PreviewCheckBoxChanged();
				};
			}
			FormTools.AddCheckBox(this, "显示断开连接的缩略图(&D)", settings.ShowDisconnectedThumbnails, 0, ref rowIndex, ref num);
		}

		private void PreviewCheckBoxChanged() {
			_interactionCheckBox.Enabled = _previewCheckBox.Checked && _previewCheckBox.Enabled;
		}

		protected override void UpdateControls() {
			base.UpdateControls();
			PreviewCheckBoxChanged();
		}
	}
}

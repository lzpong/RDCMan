namespace RdcMan {
	public class SecuritySettingsTabPage : SettingsTabPage<SecuritySettings> {
		public SecuritySettingsTabPage(TabbedSettingsDialog dialog, SecuritySettings settings)
			: base(dialog, settings) {
			int num = 0;
			int rowIndex = 0;
			CreateInheritanceControl(ref rowIndex, ref num);
			FormTools.AddLabeledEnumDropDown(this, "�����֤(&A)", settings.AuthenticationLevel, ref rowIndex, ref num, RdpClient.AuthenticationLevelToString);
			base.Controls.Add(FormTools.NewLabel("�ض��򱣻�", 0, rowIndex));
			RdcCheckBox restrictedAdmin = FormTools.AddCheckBox(this, "���޹���Ա(&R)", settings.RestrictedAdmin, 1, ref rowIndex, ref num);
			RdcCheckBox remoteGuard = FormTools.AddCheckBox(this, "Զ��ƾ�ݱ���(&G)", settings.RemoteGuard, 1, ref rowIndex, ref num);
			restrictedAdmin.CheckedChanged += delegate {
				if (restrictedAdmin.Checked)
					remoteGuard.Checked = false;
			};
			remoteGuard.CheckedChanged += delegate {
				if (remoteGuard.Checked)
					restrictedAdmin.Checked = false;
			};
		}
	}
}

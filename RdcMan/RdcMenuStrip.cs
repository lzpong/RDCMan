using System.Windows.Forms;
using Win32;

namespace RdcMan {
	public class RdcMenuStrip : MenuStrip {
		public bool IsActive { get; private set; }

		public RdcMenuStrip() {
			base.MenuActivate += delegate {
				IsActive = true;
			};
			base.MenuDeactivate += delegate {
				IsActive = false;
				if (Program.Preferences.HideMainMenu) {
					bool flag = (User.GetAsyncKeyState(164) & 0x8000) != 0;
					bool flag2 = (User.GetAsyncKeyState(165) & 0x8000) != 0;
					if (!flag && !flag2)
						((RdcBaseForm)FindForm()).SetMainMenuVisibility(show: false);
				}
			};
		}
	}
}

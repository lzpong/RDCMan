using System.Windows.Forms;

namespace RdcMan
{
	public class HotKeyBox : TextBox, ISettingControl
	{
		private Keys _hotKey;

		public string Prefix { get; set; }

		public EnumSetting<Keys> Setting { get; set; }

		public Keys HotKey
		{
			get => _hotKey;
			set
			{
				_hotKey = value;
				Text = string.Concat(str1: (HotKey == Keys.Next) ? "PageDown" : ((HotKey == Keys.Cancel) ? "Break" : ((HotKey >= Keys.D0 && HotKey <= Keys.D9) ? ((int)(HotKey - 48)).ToString() : ((HotKey < Keys.NumPad0 || HotKey > Keys.NumPad9) ? HotKey.ToString() : ((int)(HotKey - 96) + " (num pad)")))), str0: Prefix).ToUpper();
			}
		}

		protected override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			Keys keys = keyData & Keys.KeyCode;
			if (keys == Keys.Tab || keys == Keys.Escape)
			{
				return base.ProcessCmdKey(ref m, keyData);
			}
			if ((keyData & Keys.Modifiers) == 0 || keys == Keys.Cancel)
			{
				HotKey = keys;
			}
			return true;
		}

		void ISettingControl.UpdateControl()
		{
			if (Setting != null)
			{
				HotKey = Setting.Value;
			}
		}

		void ISettingControl.UpdateSetting()
		{
			if (Setting != null)
			{
				Setting.Value = HotKey;
			}
		}

		string ISettingControl.Validate()
		{
			return null;
		}
	}
}

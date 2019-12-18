using System;
using System.Windows.Forms;

namespace RdcMan
{
	public class NumericTextBox : TextBox, ISettingControl
	{
		private readonly int _min;

		private readonly int _max;

		private readonly string _invalidMessage;

		public IntSetting Setting
		{
			get;
			set;
		}

		public NumericTextBox(int min, int max, string invalidMessage)
		{
			if (min < 0)
			{
				throw new ArgumentOutOfRangeException("min");
			}
			if (max < 0)
			{
				throw new ArgumentOutOfRangeException("max");
			}
			if (min >= max)
			{
				throw new ArgumentException("最小值必须小于最大值");
			}
			if (string.IsNullOrWhiteSpace(invalidMessage))
			{
				throw new ArgumentOutOfRangeException("invalidMessage");
			}
			_min = min;
			_max = max;
			_invalidMessage = invalidMessage;
		}

		void ISettingControl.UpdateControl()
		{
			if (Setting != null)
			{
				Text = Setting.Value.ToString();
			}
		}

		void ISettingControl.UpdateSetting()
		{
			if (Setting != null)
			{
				Setting.Value = int.Parse(Text);
			}
		}

		string ISettingControl.Validate()
		{
			string result = null;
			int num = int.Parse(Text);
			if (num < _min || num > _max)
			{
				result = _invalidMessage;
			}
			return result;
		}

		protected override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			if ((keyData & Keys.Modifiers) == 0)
			{
				switch (keyData)
				{
				case Keys.D0:
				case Keys.D1:
				case Keys.D2:
				case Keys.D3:
				case Keys.D4:
				case Keys.D5:
				case Keys.D6:
				case Keys.D7:
				case Keys.D8:
				case Keys.D9:
				case Keys.NumPad0:
				case Keys.NumPad1:
				case Keys.NumPad2:
				case Keys.NumPad3:
				case Keys.NumPad4:
				case Keys.NumPad5:
				case Keys.NumPad6:
				case Keys.NumPad7:
				case Keys.NumPad8:
				case Keys.NumPad9:
					return base.ProcessCmdKey(ref m, keyData);
				}
			}
			if ((keyData & (Keys.Control | Keys.Alt)) != 0)
			{
				return base.ProcessCmdKey(ref m, keyData);
			}
			switch (keyData & Keys.KeyCode)
			{
			case Keys.Back:
			case Keys.Tab:
			case Keys.Return:
			case Keys.Escape:
			case Keys.End:
			case Keys.Home:
			case Keys.Left:
			case Keys.Right:
			case Keys.Delete:
				return base.ProcessCmdKey(ref m, keyData);
			default:
				return true;
			}
		}
	}
}

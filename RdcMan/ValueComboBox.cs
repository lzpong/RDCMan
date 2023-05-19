using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RdcMan
{
	public class ValueComboBox<TValue> : ComboBox, ISettingControl
	{
		private class ComboBoxItem<T>
		{
			public string Name;

			public T Value;

			public override string ToString()
			{
				return Name;
			}
		}

		public Setting<TValue> Setting;

		public new TValue SelectedValue
		{
			get
			{
				if (base.SelectedItem == null)
				{
					return default(TValue);
				}
				return (base.SelectedItem as ComboBoxItem<TValue>).Value;
			}
			set
			{
				int num = FindItem(value);
				if (num != -1)
				{
					SelectedIndex = num;
				}
			}
		}

		public int ItemCount => base.Items.Count;

		public new ObjectCollection Items
		{
			set
			{
				throw new InvalidOperationException();
			}
		}

		public new string Text
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public ValueComboBox(Setting<TValue> setting, IEnumerable<TValue> values, Func<TValue, string> toString)
			: this(values, toString)
		{
			Setting = setting;
		}

		public ValueComboBox(IEnumerable<TValue> values, Func<TValue, string> toString)
		{
			base.DropDownStyle = ComboBoxStyle.DropDownList;
			if (values != null)
			{
				AddItems(values, toString);
			}
		}

		public void AddItems(IEnumerable<TValue> values, Func<TValue, string> toString)
		{
			values.ForEach(delegate(TValue v)
			{
				AddItem(toString(v), v);
			});
		}

		public void AddItem(string name, TValue value)
		{
			base.Items.Add(new ComboBoxItem<TValue>
			{
				Name = name,
				Value = value
			});
		}

		public void ClearItems()
		{
			base.Items.Clear();
		}

		public void ReplaceItem(string name, TValue newValue)
		{
			int num = FindItem(name);
			if (num != -1)
			{
				ComboBoxItem<TValue> comboBoxItem = base.Items[num] as ComboBoxItem<TValue>;
				comboBoxItem.Value = newValue;
			}
		}

		public int FindItem(TValue value)
		{
			for (int i = 0; i < base.Items.Count; i++)
			{
				ComboBoxItem<TValue> comboBoxItem = base.Items[i] as ComboBoxItem<TValue>;
				if (object.Equals(comboBoxItem.Value, value))
				{
					return i;
				}
			}
			return -1;
		}

		public int FindItem(string name)
		{
			for (int i = 0; i < base.Items.Count; i++)
			{
				ComboBoxItem<TValue> comboBoxItem = base.Items[i] as ComboBoxItem<TValue>;
				if (comboBoxItem.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		void ISettingControl.UpdateControl()
		{
			if (Setting != null)
			{
				SelectedValue = Setting.Value;
			}
		}

		void ISettingControl.UpdateSetting()
		{
			if (Setting != null)
			{
				Setting.Value = SelectedValue;
			}
		}

		string ISettingControl.Validate()
		{
			return null;
		}
	}
}

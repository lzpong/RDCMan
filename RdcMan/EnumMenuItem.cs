namespace RdcMan
{
	internal abstract class EnumMenuItem<T> : RdcMenuItem
	{
		protected abstract T Value
		{
			get;
			set;
		}

		protected EnumMenuItem(string text, T value)
		{
			Text = text;
			base.Tag = value;
		}

		protected override void OnClick()
		{
			Value = (T)base.Tag;
			Program.Preferences.NeedToSave = true;
		}

		public override void Update()
		{
			base.Checked = base.Tag.Equals(Value);
		}
	}
}

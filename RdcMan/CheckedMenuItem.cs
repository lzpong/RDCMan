namespace RdcMan
{
	internal abstract class CheckedMenuItem : RdcMenuItem
	{
		protected CheckedMenuItem(string text)
		{
			Text = text;
		}

		protected sealed override void OnClick()
		{
			CheckChanged(!base.Checked);
		}

		protected abstract void CheckChanged(bool isChecked);
	}
}

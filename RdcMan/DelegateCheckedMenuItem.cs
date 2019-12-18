using System;

namespace RdcMan
{
	internal class DelegateCheckedMenuItem : CheckedMenuItem
	{
		private readonly Func<bool> _initDelegate;

		private readonly Action<bool> _changedDelegate;

		public DelegateCheckedMenuItem(string text, MenuNames name, Func<bool> initDelegate, Action<bool> changedDelegate)
			: base(text)
		{
			base.Name = name.ToString();
			_initDelegate = initDelegate;
			_changedDelegate = changedDelegate;
		}

		protected sealed override void CheckChanged(bool isChecked)
		{
			_changedDelegate(isChecked);
		}

		public sealed override void Update()
		{
			base.Checked = _initDelegate();
		}
	}
}

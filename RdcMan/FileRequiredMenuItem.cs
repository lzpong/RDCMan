using System;

namespace RdcMan
{
	internal class FileRequiredMenuItem : RdcMenuItem
	{
		private readonly Action _clickDelegate;

		public FileRequiredMenuItem(string text, MenuNames name, Action clickDelegate)
			: base(text)
		{
			base.Name = name.ToString();
			_clickDelegate = clickDelegate;
		}

		public FileRequiredMenuItem(string text, MenuNames name, string shortcut, Action clickDelegate)
			: this(text, name, clickDelegate)
		{
			base.ShortcutKeyDisplayString = shortcut;
		}

		public override void Update()
		{
			Enabled = ServerTree.Instance.AnyOpenedEditableFiles();
		}

		protected override void OnClick()
		{
			_clickDelegate();
		}
	}
}

using System;

namespace RdcMan
{
	internal class SelectedNodeMenuItem<T> : RdcMenuItem where T : RdcTreeNode
	{
		private readonly Action<T> _action;

		public SelectedNodeMenuItem(string text, MenuNames name, Action<T> action)
			: base(text)
		{
			base.Name = name.ToString();
			_action = action;
		}

		public SelectedNodeMenuItem(string text, MenuNames name, string shortcut, Action<T> action)
			: this(text, name, action)
		{
			base.ShortcutKeyDisplayString = shortcut;
		}

		public override void Update()
		{
			Enabled = (Program.TheForm.GetSelectedNode() is T);
		}

		protected override void OnClick()
		{
			_action(Program.TheForm.GetSelectedNode() as T);
		}
	}
	internal class SelectedNodeMenuItem : SelectedNodeMenuItem<RdcTreeNode>
	{
		public SelectedNodeMenuItem(string text, MenuNames name, Action<RdcTreeNode> action)
			: base(text, name, action)
		{
		}

		public SelectedNodeMenuItem(string text, MenuNames name, string shortcut, Action<RdcTreeNode> action)
			: base(text, name, shortcut, action)
		{
		}
	}
}

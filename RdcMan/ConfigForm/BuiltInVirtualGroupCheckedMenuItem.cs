namespace RdcMan
{
	internal class BuiltInVirtualGroupCheckedMenuItem : CheckedMenuItem
	{
		private IBuiltInVirtualGroup _group;

		public BuiltInVirtualGroupCheckedMenuItem(IBuiltInVirtualGroup group)
			: base(group.Text)
		{
			_group = group;
		}

		protected override void CheckChanged(bool isChecked)
		{
			_group.IsInTree = isChecked;
		}

		public override void Update()
		{
			base.Checked = _group.IsInTree;
		}
	}
}

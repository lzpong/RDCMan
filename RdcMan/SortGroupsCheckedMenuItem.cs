namespace RdcMan
{
	internal class SortGroupsCheckedMenuItem : EnumMenuItem<SortOrder>
	{
		protected override SortOrder Value
		{
			get
			{
				return Program.Preferences.GroupSortOrder;
			}
			set
			{
				Program.Preferences.GroupSortOrder = value;
				ServerTree.Instance.SortAllNodes();
				ServerTree.Instance.OnGroupChanged(ServerTree.Instance.RootNode, ChangeType.PropertyChanged);
			}
		}

		public SortGroupsCheckedMenuItem(string text, SortOrder value) : base(text, value) { }
	}
}

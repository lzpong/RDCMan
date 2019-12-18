using System;

namespace RdcMan
{
	public class GroupChangedEventArgs : EventArgs
	{
		public GroupBase Group
		{
			get;
			private set;
		}

		public ChangeType ChangeType
		{
			get;
			private set;
		}

		public GroupChangedEventArgs(GroupBase group, ChangeType changeType)
		{
			Group = group;
			ChangeType = changeType;
		}
	}
}

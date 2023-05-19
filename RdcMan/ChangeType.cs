using System;

namespace RdcMan
{
	[Flags]
	public enum ChangeType
	{
		InvalidateUI = 1,
		TreeChanged = 3,
		PropertyChanged = 5
	}
}

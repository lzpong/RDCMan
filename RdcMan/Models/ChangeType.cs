using System;

namespace RdcMan
{
	[Flags]
	public enum ChangeType
	{
		InvalidateUI = 0x1,
		TreeChanged = 0x3,
		PropertyChanged = 0x5
	}
}

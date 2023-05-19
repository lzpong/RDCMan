using System;

namespace RdcMan
{
	[Flags]
	internal enum OperationBehavior
	{
		None = 0,
		SuspendSelect = 1,
		SuspendSort = 2,
		SuspendUpdate = 4,
		SuspendGroupChanged = 8,
		RestoreSelected = 0x11
	}
}

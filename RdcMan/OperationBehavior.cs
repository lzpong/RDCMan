using System;

namespace RdcMan
{
	[Flags]
	internal enum OperationBehavior
	{
		None = 0x0,
		SuspendSelect = 0x1,
		SuspendSort = 0x2,
		SuspendUpdate = 0x4,
		SuspendGroupChanged = 0x8,
		RestoreSelected = 0x11
	}
}

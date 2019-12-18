using System;

namespace RdcMan
{
	[Flags]
	public enum LogonCredentialsDialogOptions
	{
		None = 0x0,
		AllowInheritance = 0x1,
		ShowProfiles = 0x2,
		All = 0xFF
	}
}

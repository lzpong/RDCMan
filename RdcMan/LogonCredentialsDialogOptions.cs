using System;

namespace RdcMan {
	[Flags]
	public enum LogonCredentialsDialogOptions {
		None = 0,
		AllowInheritance = 1,
		ShowProfiles = 2,
		All = 0xFF
	}
}

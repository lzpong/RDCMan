using System.Runtime.InteropServices;

namespace Win32
{
	public class Util
	{
		public const uint MvkVkeyToScanCode = 0u;

		[DllImport("user32.dll")]
		public static extern uint MapVirtualKey(uint uCode, uint uMapType);
	}
}

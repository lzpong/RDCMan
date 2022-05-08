using System.Runtime.InteropServices;

namespace Win32 {
	public class Kernel {
		public static uint MajorVersion { get; private set; }

		public static uint MinorVersion { get; private set; }

		public static uint Build { get; private set; }

		static Kernel() {
			uint version = GetVersion();
			MajorVersion = version & 0xFFu;
			MinorVersion = (version & 0xFF00) >> 8;
			Build = (version & 0xFFFF0000u) >> 16;
		}

		[DllImport("kernel32.dll")]
		public static extern int GetLastError();

		[DllImport("kernel32")]
		public static extern uint GetVersion();
	}
}

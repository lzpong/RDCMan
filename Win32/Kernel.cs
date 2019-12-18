using System.Runtime.InteropServices;

namespace Win32
{
	public class Kernel
	{
		public static uint MajorVersion
		{
			get;
			private set;
		}

		public static uint MinorVersion
		{
			get;
			private set;
		}

		public static uint Build
		{
			get;
			private set;
		}

		static Kernel()
		{
			uint version = GetVersion();
			MajorVersion = (version & 0xFF);
			MinorVersion = (version & 0xFF00) >> 8;
			Build = (uint)((int)version & -65536) >> 16;
		}

		[DllImport("kernel32.dll")]
		public static extern int GetLastError();

		[DllImport("kernel32")]
		public static extern uint GetVersion();
	}
}

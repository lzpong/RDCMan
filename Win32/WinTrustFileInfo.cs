using System;
using System.Runtime.InteropServices;

namespace Win32
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class WinTrustFileInfo
	{
		private uint StructSize = (uint)Marshal.SizeOf(typeof(WinTrustFileInfo));

		private IntPtr pszFilePath;

		private IntPtr hFile = IntPtr.Zero;

		private IntPtr pgKnownSubject = IntPtr.Zero;

		public WinTrustFileInfo(string _filePath)
		{
			pszFilePath = Marshal.StringToCoTaskMemAuto(_filePath);
		}

		~WinTrustFileInfo()
		{
			Marshal.FreeCoTaskMem(pszFilePath);
		}
	}
}

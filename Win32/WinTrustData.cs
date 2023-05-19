using System;
using System.Runtime.InteropServices;

namespace Win32 {
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class WinTrustData {
		private uint StructSize = (uint)Marshal.SizeOf(typeof(WinTrustData));

		private IntPtr PolicyCallbackData = IntPtr.Zero;

		private IntPtr SIPClientData = IntPtr.Zero;

		private WinTrustDataUIChoice UIChoice = WinTrustDataUIChoice.None;

		private WinTrustDataRevocationChecks RevocationChecks;

		private WinTrustDataChoice UnionChoice = WinTrustDataChoice.File;

		private IntPtr FileInfoPtr;

		private WinTrustDataStateAction StateAction;

		private IntPtr StateData = IntPtr.Zero;

		private string URLReference;

		private WinTrustDataProvFlags ProvFlags = WinTrustDataProvFlags.RevocationCheckChainExcludeRoot;

		private WinTrustDataUIContext UIContext;

		public WinTrustData(string _fileName) {
			if (Environment.OSVersion.Version.Major > 6 
				|| (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 1) 
				|| (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1 && !string.IsNullOrEmpty(Environment.OSVersion.ServicePack))
				) {
				ProvFlags |= WinTrustDataProvFlags.DisableMD2andMD4;
			}
			WinTrustFileInfo structure = new WinTrustFileInfo(_fileName);
			FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WinTrustFileInfo)));
			Marshal.StructureToPtr(structure, FileInfoPtr, fDeleteOld: false);
		}

		~WinTrustData() {
			Marshal.FreeCoTaskMem(FileInfoPtr);
		}
	}
}

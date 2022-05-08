using System;
using System.Runtime.InteropServices;

namespace Win32 {
	public sealed class WinTrust {
		private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

		private const string WINTRUST_ACTION_GENERIC_VERIFY_V2 = "{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}";

		[DllImport("wintrust.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		private static extern WinVerifyTrustResult WinVerifyTrust([In] IntPtr hwnd, [In][MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID, [In] WinTrustData pWVTData);

		public static bool VerifyEmbeddedSignature(string fileName) {
			WinTrustData pWVTData = new WinTrustData(fileName);
			WinVerifyTrustResult winVerifyTrustResult = WinVerifyTrust(pgActionID: new Guid(WINTRUST_ACTION_GENERIC_VERIFY_V2), hwnd: INVALID_HANDLE_VALUE, pWVTData: pWVTData);
			return winVerifyTrustResult == WinVerifyTrustResult.Success;
		}

		private WinTrust() { }
	}
}

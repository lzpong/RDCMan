using System;
using System.Runtime.InteropServices;

namespace Win32 {
	public class Wts {
		public enum ConnectstateClass {
			Active,
			Connected,
			ConnectQuery,
			Shadow,
			Disconnected,
			Idle,
			Listen,
			Reset,
			Down,
			Init
		}

		public enum InfoClass {
			InitialProgram,
			ApplicationName,
			WorkingDirectory,
			OEMId,
			SessionId,
			UserName,
			WinStationName,
			DomainName,
			ConnectState,
			ClientBuildNumber,
			ClientName,
			ClientDirectory,
			ClientProductId,
			ClientHardwareId,
			ClientAddress,
			ClientDisplay,
			ClientProtocolType,
			IdleTime,
			LogonTime,
			IncomingBytes,
			OutgoingBytes,
			IncomingFrames,
			OutgoingFrames
		}

		public enum ShutdownMode {
			Reboot = 4,
			PowerOff = 8
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class SessionInfo {
			public int SessionId;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string WinStationName;

			public ConnectstateClass State;
		}

		[DllImport("wtsapi32.dll", EntryPoint = "WTSOpenServer")]
		public static extern IntPtr OpenServer(string serverName);

		[DllImport("wtsapi32.dll", EntryPoint = "WTSCloseServer")]
		public static extern void CloseServer(IntPtr hServer);

		[DllImport("wtsapi32.dll", EntryPoint = "WTSEnumerateSessionsW")]
		public static extern bool EnumerateSessions(IntPtr hServer, int reserved, int version, out IntPtr pSessionInfo, out int count);

		[DllImport("wtsapi32.dll", EntryPoint = "WTSFreeMemory")]
		public static extern void FreeMemory(IntPtr pMemory);

		[DllImport("wtsapi32.dll", EntryPoint = "WTSQuerySessionInformationW")]
		public static extern bool QuerySessionInformation(IntPtr hServer, int sessionId, InfoClass infoClass, out IntPtr pBuffer, out int bytesReturned);

		[DllImport("wtsapi32.dll", EntryPoint = "WTSDisconnectSession")]
		public static extern bool DisconnectSession(IntPtr hServer, int sessionId, bool wait);

		[DllImport("wtsapi32.dll", EntryPoint = "WTSLogoffSession")]
		public static extern bool LogOffSession(IntPtr hServer, int sessionId, bool wait);

		[DllImport("wtsapi32.dll", EntryPoint = "WTSShutdownSystem")]
		public static extern bool ShutdownSystem(IntPtr hServer, int mode);
	}
}

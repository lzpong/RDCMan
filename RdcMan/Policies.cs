using Microsoft.Win32;

namespace RdcMan {
	public class Policies {
		public const string PolicyRegKey = "RDCMan";

		public static bool DisableLogOff;

		public static void Read() {
			try {
				RegistryKey registryKey = Registry.LocalMachine
					.OpenSubKey("Software", writable: true)
					.OpenSubKey("Policies", writable: true)
					.OpenSubKey("Microsoft", writable: true)
					.CreateSubKey(PolicyRegKey, RegistryKeyPermissionCheck.ReadSubTree);
				if (registryKey != null)
					DisableLogOff = (int)registryKey.GetValue("DisableLogOff", 0) == 1;
			}
			catch { }
		}
	}
}

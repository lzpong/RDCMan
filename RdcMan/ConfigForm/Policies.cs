using Microsoft.Win32;

namespace RdcMan
{
	public class Policies
	{
		public const string PolicyRegKey = "RDCMan";
		/// <summary>
		/// 是否禁用注销/登出
		/// </summary>
		public static bool DisableLogOff;

		public static void Read()
		{
			try
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software", writable: true).OpenSubKey("Policies", writable: true).OpenSubKey("Microsoft", writable: true)
					.CreateSubKey("RDCMan", RegistryKeyPermissionCheck.ReadSubTree);
				if (registryKey != null)
				{
					DisableLogOff = ((int)registryKey.GetValue("DisableLogOff", 0) == 1);
				}
			}
			catch
			{
			}
		}
	}
}

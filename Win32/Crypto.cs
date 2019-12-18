using System;
using System.Runtime.InteropServices;

namespace Win32
{
	public class Crypto
	{
		public struct DataBlob
		{
			public int Size;

			public IntPtr Data;
		}

		public struct CryptProtectPromptStruct
		{
			public int Size;

			public int Flags;

			public IntPtr Window;

			public string Message;
		}

		[DllImport("crypt32.dll", CharSet = CharSet.Unicode)]
		public static extern bool CryptProtectData(ref DataBlob dataIn, string description, ref DataBlob optionalEntropy, IntPtr reserved, ref CryptProtectPromptStruct promptStruct, int flags, out DataBlob dataOut);

		[DllImport("crypt32.dll", CharSet = CharSet.Unicode)]
		public static extern bool CryptUnprotectData(ref DataBlob dataIn, string description, ref DataBlob optionalEntropy, IntPtr reserved, ref CryptProtectPromptStruct promptStruct, int flags, out DataBlob dataOut);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern void LocalFree(IntPtr ptr);
	}
}

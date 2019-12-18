using System;

namespace Win32
{
	[Flags]
	public enum WinTrustDataProvFlags : uint
	{
		UseIe4TrustFlag = 0x1,
		NoIe4ChainFlag = 0x2,
		NoPolicyUsageFlag = 0x4,
		RevocationCheckNone = 0x10,
		RevocationCheckEndCert = 0x20,
		RevocationCheckChain = 0x40,
		RevocationCheckChainExcludeRoot = 0x80,
		SaferFlag = 0x100,
		HashOnlyFlag = 0x200,
		UseDefaultOsverCheck = 0x400,
		LifetimeSigningFlag = 0x800,
		CacheOnlyUrlRetrieval = 0x1000,
		DisableMD2andMD4 = 0x2000
	}
}

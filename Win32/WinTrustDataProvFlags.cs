using System;

namespace Win32 {
	[Flags]
	public enum WinTrustDataProvFlags : uint {
		UseIe4TrustFlag = 1u,
		NoIe4ChainFlag = 2u,
		NoPolicyUsageFlag = 4u,
		RevocationCheckNone = 0x10u,
		RevocationCheckEndCert = 0x20u,
		RevocationCheckChain = 0x40u,
		RevocationCheckChainExcludeRoot = 0x80u,
		SaferFlag = 0x100u,
		HashOnlyFlag = 0x200u,
		UseDefaultOsverCheck = 0x400u,
		LifetimeSigningFlag = 0x800u,
		CacheOnlyUrlRetrieval = 0x1000u,
		DisableMD2andMD4 = 0x2000u
	}
}

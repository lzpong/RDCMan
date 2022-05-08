namespace Win32 {
	public enum WinVerifyTrustResult : uint {
		Success = 0u,
		ProviderUnknown = 2148204545u,
		ActionUnknown = 2148204546u,
		SubjectFormUnknown = 2148204547u,
		SubjectNotTrusted = 2148204548u,
		FileNotSigned = 2148204800u,
		SubjectExplicitlyDistrusted = 2148204817u,
		SignatureOrFileCorrupt = 2148098064u,
		SubjectCertExpired = 2148204801u,
		SubjectCertificateRevoked = 134262800u
	}
}

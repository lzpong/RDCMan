namespace Win32 {
	public enum WinTrustDataChoice : uint {
		File = 1u,
		Catalog,
		Blob,
		Signer,
		Certificate
	}
}

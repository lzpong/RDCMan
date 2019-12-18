namespace RdcMan
{
	public interface IDeferDecryption
	{
		bool IsDecrypted
		{
			get;
			set;
		}

		void Decrypt(EncryptionSettings settings);
	}
}

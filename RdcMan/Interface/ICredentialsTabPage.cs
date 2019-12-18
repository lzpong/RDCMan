namespace RdcMan
{
	public interface ICredentialsTabPage
	{
		CredentialsProfile Credentials
		{
			get;
		}

		void PopulateCredentialsProfiles(FileGroup file);
	}
}

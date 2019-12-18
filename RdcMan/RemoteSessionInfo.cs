using Win32;

namespace RdcMan
{
	internal class RemoteSessionInfo
	{
		public string ClientName;

		public string DomainName;

		public int SessionId;

		public Wts.ConnectstateClass State;

		public string UserName;
	}
}

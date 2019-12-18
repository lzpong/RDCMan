using System;

namespace RdcMan
{
	internal class ConnectedServerRef : ServerRef
	{
		public DateTime LastFocusTime
		{
			get;
			set;
		}

		public ConnectedServerRef(Server server)
			: base(server)
		{
		}

		public override bool ConfirmRemove(bool askUser)
		{
			FormTools.InformationDialog("断开此服务器，将其从“Connected”组中删除");
			return false;
		}
	}
}

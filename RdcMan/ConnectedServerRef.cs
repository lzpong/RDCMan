using System;

namespace RdcMan
{
	internal class ConnectedServerRef : ServerRef
	{
		public DateTime LastFocusTime { get; set; }

		public ConnectedServerRef(Server server)
			: base(server)
		{
		}

		public override bool ConfirmRemove(bool askUser)
		{
			FormTools.InformationDialog("断开服务器以将其从 已连接 组中删除");
			return false;
		}
	}
}

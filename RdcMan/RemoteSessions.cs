using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Win32;

namespace RdcMan
{
	internal class RemoteSessions
	{
		private IntPtr _hServer;

		private readonly ServerBase _server;

		public RemoteSessions(ServerBase server)
		{
			_server = server;
			_hServer = (IntPtr)0;
		}

		public bool OpenServer()
		{
			_hServer = Wts.OpenServer(_server.ServerName);
			if (_hServer == (IntPtr)0)
			{
				return false;
			}
			return true;
		}

		public void CloseServer()
		{
			if (_hServer != (IntPtr)0)
			{
				Wts.CloseServer(_hServer);
				_hServer = (IntPtr)0;
			}
		}

		public IList<RemoteSessionInfo> QuerySessions()
		{
			if (_hServer == (IntPtr)0)
			{
				throw new Exception("QuerySessions called before OpenServer succeeded");
			}
			if (!Wts.EnumerateSessions(_hServer, 0, 1, out IntPtr pSessionInfo, out int count))
			{
				return null;
			}
			List<RemoteSessionInfo> list = new List<RemoteSessionInfo>();
			Wts.SessionInfo sessionInfo = new Wts.SessionInfo();
			try
			{
				IntPtr intPtr = pSessionInfo;
				for (int i = 0; i < count; i++)
				{
					Marshal.PtrToStructure(intPtr, (object)sessionInfo);
					intPtr = (IntPtr)((long)intPtr + Marshal.SizeOf((object)sessionInfo));
					string text = null;
					string text2 = null;
					string text3 = null;
					Wts.QuerySessionInformation(_hServer, sessionInfo.SessionId, Wts.InfoClass.UserName, out IntPtr pBuffer, out int bytesReturned);
					text = Marshal.PtrToStringAuto(pBuffer);
					if (text.Length != 0)
					{
						Wts.QuerySessionInformation(_hServer, sessionInfo.SessionId, Wts.InfoClass.DomainName, out pBuffer, out bytesReturned);
						text2 = Marshal.PtrToStringAuto(pBuffer);
						Wts.QuerySessionInformation(_hServer, sessionInfo.SessionId, Wts.InfoClass.ClientName, out pBuffer, out bytesReturned);
						text3 = Marshal.PtrToStringAuto(pBuffer);
						list.Add(new RemoteSessionInfo
						{
							ClientName = text3,
							DomainName = text2,
							SessionId = sessionInfo.SessionId,
							UserName = text,
							State = sessionInfo.State
						});
					}
				}
				return list;
			}
			finally
			{
				Wts.FreeMemory(pSessionInfo);
			}
		}

		public bool DisconnectSession(int id)
		{
			return Wts.DisconnectSession(_hServer, id, wait: true);
		}

		public bool LogOffSession(int id)
		{
			return Wts.LogOffSession(_hServer, id, wait: true);
		}
	}
}

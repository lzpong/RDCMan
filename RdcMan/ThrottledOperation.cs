using System;
using System.Collections.Generic;

namespace RdcMan
{
	internal class ThrottledOperation : IDisposable
	{
		private HashSet<Server> _serversInScope;

		private object _serversInScopeLock = new object();

		private ThrottledAction _throttledAction;

		private HashSet<RdpClient.ConnectionState> _completionStates;

		private bool _disposed;

		public ThrottledOperation(List<ServerBase> servers, IEnumerable<RdpClient.ConnectionState> completionStates, Action preAction, Action<ServerBase> action, int delayInMilliseconds, Action postAction)
		{
			_serversInScope = new HashSet<Server>();
			_completionStates = new HashSet<RdpClient.ConnectionState>(completionStates);
			Action preAction2 = delegate
			{
				preAction();
				Server.ConnectionStateChanged += ConnectionStateChangeConnectHandler;
			};
			_throttledAction = new ThrottledAction(servers, preAction2, delegate(ServerBase server)
			{
				lock (_serversInScopeLock)
				{
					_serversInScope.Add(server.ServerNode);
				}
				action(server);
			}, delayInMilliseconds, delegate
			{
				Server.ConnectionStateChanged -= ConnectionStateChangeConnectHandler;
				postAction();
			});
		}

		~ThrottledOperation()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing && _throttledAction != null)
				{
					_throttledAction.Dispose();
					_throttledAction = null;
				}
				_disposed = true;
			}
		}

		public void Execute()
		{
			_throttledAction.Execute();
		}

		private void ConnectionStateChangeConnectHandler(ConnectionStateChangedEventArgs args)
		{
			if (_completionStates.Contains(args.State))
			{
				bool flag;
				lock (_serversInScopeLock)
				{
					flag = _serversInScope.Remove(args.Server);
				}
				if (flag)
				{
					_throttledAction.CompleteAction();
				}
			}
		}
	}
}

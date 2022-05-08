using System;
using System.Collections.Generic;
using System.Threading;

namespace RdcMan {
	internal class ThrottledAction : IDisposable {
		private const int Max = 3;

		private int _numActive;

		private bool _disposed;

		private Semaphore _actionSemaphore;

		private readonly List<ServerBase> _servers;

		private Action _preAction;

		private readonly Action<ServerBase> _action;

		private int _delayInMilliseconds;

		private readonly Action _postAction;

		public ThrottledAction(List<ServerBase> servers, Action preAction, Action<ServerBase> action, int delayInMilliseconds, Action postAction) {
			_servers = servers;
			_preAction = preAction;
			_action = action;
			_delayInMilliseconds = delayInMilliseconds;
			_postAction = postAction;
		}

		~ThrottledAction() {
			Dispose(disposing: false);
		}

		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void Execute() {
			_numActive = 0;
			using (_actionSemaphore = new Semaphore(3, Max)) {
				try {
					_preAction();
					foreach (ServerBase server in _servers) {
						_actionSemaphore.WaitOne();
						Interlocked.Increment(ref _numActive);
						ThreadPool.QueueUserWorkItem(delegate (object s) {
							_action(s as ServerBase);
						}, server);
						Thread.Sleep(_delayInMilliseconds);
					}
					WaitForCompletion();
				}
				finally {
					_postAction();
				}
			}
		}

		public void CompleteAction() {
			_actionSemaphore.Release();
			Interlocked.Decrement(ref _numActive);
		}

		private void WaitForCompletion() {
			while (Thread.VolatileRead(ref _numActive) > 0) {
				Thread.Sleep(_delayInMilliseconds);
			}
		}

		protected virtual void Dispose(bool disposing) {
			if (!_disposed) {
				if (disposing && _actionSemaphore != null) {
					_actionSemaphore.Close();
					_actionSemaphore = null;
				}
				_disposed = true;
			}
		}
	}
}

using System;
using System.Collections.Generic;

namespace RdcMan {
	public class EnableTabsEventArgs : EventArgs {
		public bool Enabled;

		public string Reason;

		public IEnumerable<string> TabNames;
	}
}

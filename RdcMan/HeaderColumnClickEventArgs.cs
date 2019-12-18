using System;

namespace RdcMan
{
	public class HeaderColumnClickEventArgs : EventArgs
	{
		public int Column
		{
			get;
			private set;
		}

		public bool IsChecked
		{
			get;
			private set;
		}

		public HeaderColumnClickEventArgs(int column, bool isChecked)
		{
			Column = column;
			IsChecked = isChecked;
		}
	}
}

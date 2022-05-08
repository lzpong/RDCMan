using System;
using System.Runtime.InteropServices;

namespace Win32 {
	public class Structs {
		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct HDITEM {
			public uint mask;

			public int cxy;

			public IntPtr pszText;

			public IntPtr hbm;

			public int cchTextMax;

			public int fmt;

			public int lParam;

			public int iImage;

			public int iOrder;

			public uint type;

			public unsafe void* pvFilter;

			public uint state;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct NMHDR {
			public IntPtr hwndFrom;

			public IntPtr idFrom;

			public uint code;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct NMHEADER {
			public NMHDR hdr;

			public int iItem;

			public int iButton;

			private IntPtr pitem;
		}

		public struct RECT {
			public int left;

			public int top;

			public int right;

			public int bottom;
		}

		public struct TRACKMOUSEEVENT {
			public int cbSize;

			public uint dwFlags;

			public IntPtr hWnd;

			public uint dwHoverTime;

			public TRACKMOUSEEVENT(uint dwFlags, IntPtr hWnd, uint dwHoverTime) {
				cbSize = Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
				this.dwFlags = dwFlags;
				this.hWnd = hWnd;
				this.dwHoverTime = dwHoverTime;
			}
		}
	}
}

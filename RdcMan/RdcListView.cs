using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Win32;

namespace RdcMan
{
	public class RdcListView : ListView
	{
		public static bool SupportsHeaderCheckBoxes
		{
			get;
			private set;
		}

		private IntPtr HeaderHandle => User.SendMessage(base.Handle, 4127u, (IntPtr)0, (IntPtr)0);

		public event HeaderColumnClickEventHandler HeaderCheckBoxClick;

		static RdcListView()
		{
			SupportsHeaderCheckBoxes = (Kernel.MajorVersion >= 6);
		}

		public unsafe void SetColumnHeaderToCheckBox(int index)
		{
			if (!SupportsHeaderCheckBoxes)
			{
				throw new InvalidOperationException("Header check boxes are not supported on this operating system version");
			}
			if (base.Parent == null)
			{
				throw new InvalidOperationException("Control must have a parent before setting header style");
			}
			if (index < 0 || index >= base.Columns.Count)
			{
				throw new IndexOutOfRangeException("Column index out of range");
			}
			if (!string.IsNullOrEmpty(base.Columns[index].Text))
			{
				throw new InvalidOperationException("Column must have no text");
			}
			IntPtr headerHandle = HeaderHandle;
			int windowLong = User.GetWindowLong(headerHandle, -16);
			User.SetWindowLong(headerHandle, -16, windowLong | 0x400);
			Structs.HDITEM hDITEM = default(Structs.HDITEM);
			hDITEM.mask = 4u;
			Structs.HDITEM* value = &hDITEM;
			User.SendMessage(headerHandle, 4619u, (IntPtr)index, (IntPtr)(void*)value);
			hDITEM.fmt |= 320;
			User.SendMessage(headerHandle, 4620u, (IntPtr)index, (IntPtr)(void*)value);
		}

		public unsafe void SetColumnHeaderChecked(int index, bool isChecked)
		{
			IntPtr headerHandle = HeaderHandle;
			Structs.HDITEM hDITEM = default(Structs.HDITEM);
			hDITEM.mask = 4u;
			Structs.HDITEM* value = &hDITEM;
			User.SendMessage(headerHandle, 4619u, (IntPtr)index, (IntPtr)(void*)value);
			if (isChecked)
			{
				hDITEM.fmt |= 128;
			}
			else
			{
				hDITEM.fmt &= -129;
			}
			User.SendMessage(headerHandle, 4620u, (IntPtr)index, (IntPtr)(void*)value);
		}

		protected unsafe override void WndProc(ref Message m)
		{
			if ((long)m.Msg == 78 && this.HeaderCheckBoxClick != null)
			{
				Structs.NMHEADER nMHEADER = (Structs.NMHEADER)Marshal.PtrToStructure(m.LParam, typeof(Structs.NMHEADER));
				if (nMHEADER.hdr.code == 4294966980u)
				{
					Structs.HDITEM hDITEM = default(Structs.HDITEM);
					hDITEM.mask = 4u;
					Structs.HDITEM* value = &hDITEM;
					User.SendMessage(nMHEADER.hdr.hwndFrom, 4619u, (IntPtr)nMHEADER.iItem, (IntPtr)(void*)value);
					hDITEM.fmt ^= 128;
					User.SendMessage(nMHEADER.hdr.hwndFrom, 4620u, (IntPtr)nMHEADER.iItem, (IntPtr)(void*)value);
					bool isChecked = (hDITEM.fmt & 0x80) != 0;
					this.HeaderCheckBoxClick(this, new HeaderColumnClickEventArgs(nMHEADER.iItem, isChecked));
					return;
				}
			}
			base.WndProc(ref m);
		}
	}
}

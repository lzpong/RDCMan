using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Win32;

namespace RdcMan {
	public class RdcListView : ListView {
		public static bool SupportsHeaderCheckBoxes { get; private set; }

		private IntPtr HeaderHandle => User.SendMessage(base.Handle, 4127u, (IntPtr)0, (IntPtr)0);

		public event HeaderColumnClickEventHandler HeaderCheckBoxClick;

		static RdcListView() {
			SupportsHeaderCheckBoxes = Kernel.MajorVersion >= 6;
		}

		public unsafe void SetColumnHeaderToCheckBox(int index) {
			if (!SupportsHeaderCheckBoxes)
				throw new InvalidOperationException("此操作系统版本不支持标题复选框");
			if (base.Parent == null)
				throw new InvalidOperationException("在设置标题样式之前，控件必须有父级");
			if (index < 0 || index >= base.Columns.Count)
				throw new IndexOutOfRangeException("列索引超出范围");
			if (!string.IsNullOrEmpty(base.Columns[index].Text))
				throw new InvalidOperationException("列不能有文字");

			IntPtr headerHandle = HeaderHandle;
			int windowLong = User.GetWindowLong(headerHandle, -16);
			User.SetWindowLong(headerHandle, -16, windowLong | 0x400);
			Structs.HDITEM hDITEM = default(Structs.HDITEM);
			hDITEM.mask = 4u;
			Structs.HDITEM* ptr = &hDITEM;
			User.SendMessage(headerHandle, 4619u, (IntPtr)index, (IntPtr)ptr);
			hDITEM.fmt |= 320;
			User.SendMessage(headerHandle, 4620u, (IntPtr)index, (IntPtr)ptr);
		}

		public unsafe void SetColumnHeaderChecked(int index, bool isChecked) {
			IntPtr headerHandle = HeaderHandle;
			Structs.HDITEM hDITEM = default(Structs.HDITEM);
			hDITEM.mask = 4u;
			Structs.HDITEM* ptr = &hDITEM;
			User.SendMessage(headerHandle, 4619u, (IntPtr)index, (IntPtr)ptr);
			if (isChecked)
				hDITEM.fmt |= 128;
			else
				hDITEM.fmt &= -129;

			User.SendMessage(headerHandle, 4620u, (IntPtr)index, (IntPtr)ptr);
		}

		protected unsafe override void WndProc(ref Message m) {
			if ((long)m.Msg == 78 && this.HeaderCheckBoxClick != null) {
				Structs.NMHEADER nMHEADER = (Structs.NMHEADER)Marshal.PtrToStructure(m.LParam, typeof(Structs.NMHEADER));
				if (nMHEADER.hdr.code == 4294966980u) {
					Structs.HDITEM hDITEM = default(Structs.HDITEM);
					hDITEM.mask = 4u;
					Structs.HDITEM* ptr = &hDITEM;
					User.SendMessage(nMHEADER.hdr.hwndFrom, 4619u, (IntPtr)nMHEADER.iItem, (IntPtr)ptr);
					hDITEM.fmt ^= 128;
					User.SendMessage(nMHEADER.hdr.hwndFrom, 4620u, (IntPtr)nMHEADER.iItem, (IntPtr)ptr);
					bool isChecked = (hDITEM.fmt & 0x80) != 0;
					this.HeaderCheckBoxClick(this, new HeaderColumnClickEventArgs(nMHEADER.iItem, isChecked));
					return;
				}
			}
			base.WndProc(ref m);
		}
	}
}

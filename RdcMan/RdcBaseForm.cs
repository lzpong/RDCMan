using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Win32;

namespace RdcMan
{
	public abstract class RdcBaseForm : Form
	{
		private class DrawingControl
		{
			public static void SuspendDrawing(Control parent)
			{
				User.SendMessage(parent.Handle, 11u, (IntPtr)0, (IntPtr)0);
			}

			public static void ResumeDrawing(Control parent)
			{
				User.SendMessage(parent.Handle, 11u, (IntPtr)1, (IntPtr)0);
				parent.Refresh();
			}
		}

		private IntPtr NCButtonDownLParam;

		private const bool WindowedFullScreen = false;

		private Rectangle _savedBounds;

		private FormBorderStyle _savedBorderStyle;

		protected RdcMenuStrip _menuStrip;

		protected Panel _menuPanel;

		public bool IsActive => Form.ActiveForm == this;

		protected RdcBaseForm()
		{
			base.AutoScaleDimensions = new SizeF(96f, 96f);
			base.AutoScaleMode = AutoScaleMode.Dpi;
			_menuPanel = new Panel
			{
				Dock = DockStyle.None
			};
			_menuStrip = new RdcMenuStrip
			{
				BackColor = Color.FromKnownColor(KnownColor.Control),
				ForeColor = Color.FromKnownColor(KnownColor.ControlText),
				Visible = true
			};
			_menuStrip.MenuActivate += delegate
			{
				SetMainMenuVisibility(show: true);
				UpdateMainMenu();
			};
			_menuPanel.Controls.Add(_menuStrip);
			base.Controls.Add(_menuPanel);
		}

		public abstract void SetClientSize(Size size);

		public abstract Size GetClientSize();

		public void SetMainMenuVisibility()
		{
			SetMainMenuVisibility(!Program.Preferences.HideMainMenu);
		}

		public bool SetMainMenuVisibility(bool show)
		{
			int num = (show ? _menuStrip.Height : 0);
			if (_menuPanel.Height != num)
			{
				_menuPanel.Height = num;
				LayoutContent();
			}
			return show;
		}

		public virtual void GoFullScreenClient(Server server, bool isTopMostWindow)
		{
			RdpClient client = server.Client;
			Rectangle rectangle = Screen.GetBounds(client.Control);
			if (Program.Preferences.UseMultipleMonitors && (rectangle.Height < client.MsRdpClient.DesktopHeight || rectangle.Width < client.MsRdpClient.DesktopWidth))
			{
				int num = 0;
				int num2 = 65535;
				Screen[] allScreens = Screen.AllScreens;
				foreach (Screen screen in allScreens)
				{
					num += screen.Bounds.Width;
					num2 = Math.Min(screen.Bounds.Height, num2);
				}
				num = Math.Min(num, RdpClient.MaxDesktopWidth);
				num2 = Math.Min(num2, RdpClient.MaxDesktopHeight);
				rectangle = new Rectangle(0, 0, num, num2);
			}
			_savedBounds = base.Bounds;
			_savedBorderStyle = base.FormBorderStyle;
			DrawingControl.SuspendDrawing(this);
			SuspendLayout();
			base.FormBorderStyle = FormBorderStyle.None;
			SetMainMenuVisibility(show: false);
			SetBounds(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
			server.SetClientSizeProperties();
			client.Control.Bounds = new Rectangle(0, 0, base.Width, base.Height);
			ResumeLayout();
			base.TopMost = isTopMostWindow;
			client.Control.Show();
			DrawingControl.ResumeDrawing(this);
			BringToFront();
			Activate();
		}

		public virtual bool SwitchFullScreenClient(Server newServer)
		{
			return false;
		}

		public virtual void LeaveFullScreenClient(Server server)
		{
			DrawingControl.SuspendDrawing(this);
			SuspendLayout();
			base.FormBorderStyle = _savedBorderStyle;
			SetMainMenuVisibility();
			base.Bounds = _savedBounds;
			ResumeLayout();
			base.TopMost = false;
			DrawingControl.ResumeDrawing(this);
			server.SetClientSizeProperties();
			Activate();
		}

		protected override void OnLeave(EventArgs e)
		{
			if (Program.Preferences.HideMainMenu && _menuPanel.Height > 0)
			{
				User.SendMessage(_menuStrip.Handle, 16u, IntPtr.Zero, IntPtr.Zero);
			}
			base.OnLeave(e);
		}

		protected abstract void UpdateMainMenu();

		protected abstract void LayoutContent();

		protected void UpdateMenuItems(ToolStripItemCollection items)
		{
			foreach (ToolStripItem item in items)
			{
				if (item is RdcMenuItem)
				{
					(item as RdcMenuItem).Update();
				}
				if (item is ToolStripMenuItem toolStripMenuItem && toolStripMenuItem.DropDownItems != null)
				{
					UpdateMenuItems(toolStripMenuItem.DropDownItems);
				}
			}
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			if (Program.Preferences.HideMainMenu && m.WParam == (IntPtr)18 && (long)m.Msg == 261)
			{
				SetMainMenuVisibility(_menuPanel.Height == 0);
			}
			return base.ProcessKeyPreview(ref m);
		}

		private void SetNonClientTracking(int hoverMilliseconds)
		{
			Structs.TRACKMOUSEEVENT lpEventTrack = new Structs.TRACKMOUSEEVENT(17u, base.Handle, (uint)hoverMilliseconds);
			if (hoverMilliseconds < 0)
			{
				lpEventTrack.dwFlags |= 2147483648u;
			}
			bool flag = User.TrackMouseEvent(ref lpEventTrack);
			int lastWin32Error = Marshal.GetLastWin32Error();
		}

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			SetMainMenuVisibility();
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
			case 33:
				if (Program.Preferences.HideMainMenu)
				{
					SetMainMenuVisibility();
				}
				break;
			case 160:
				SetNonClientTracking(100);
				break;
			case 674:
				SetNonClientTracking(-1);
				break;
			case 161:
				NCButtonDownLParam = m.LParam;
				SetNonClientTracking(100);
				break;
			case 672:
				if (IsActive && Program.Preferences.HideMainMenu && m.WParam.ToInt32() == 2 && m.LParam == NCButtonDownLParam && (User.GetAsyncKeyState(1) & 0x8000) == 0)
				{
					SetMainMenuVisibility(_menuPanel.Height == 0);
					SetNonClientTracking(-1);
					NCButtonDownLParam = IntPtr.Zero;
					return;
				}
				break;
			}
			base.WndProc(ref m);
		}
	}
}

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Win32 {
	public class User {
		public struct INPUT {
			public uint type;

			public InputUnion U;

			public static int Size => Marshal.SizeOf(typeof(INPUT));
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct InputUnion {
			[FieldOffset(0)]
			public MOUSEINPUT mi;

			[FieldOffset(0)]
			public KEYBDINPUT ki;
		}

		public struct MOUSEINPUT {
			public int dx;

			public int dy;

			public MouseEventDataXButtons mouseData;

			public MOUSEEVENTF dwFlags;

			public uint time;

			public UIntPtr dwExtraInfo;
		}

		[Flags]
		public enum MouseEventDataXButtons : uint {
			Nothing = 0u,
			XBUTTON1 = 1u,
			XBUTTON2 = 2u
		}

		[Flags]
		public enum MOUSEEVENTF : uint {
			ABSOLUTE = 0x8000u,
			HWHEEL = 0x1000u,
			MOVE = 1u,
			MOVE_NOCOALESCE = 0x2000u,
			LEFTDOWN = 2u,
			LEFTUP = 4u,
			RIGHTDOWN = 8u,
			RIGHTUP = 0x10u,
			MIDDLEDOWN = 0x20u,
			MIDDLEUP = 0x40u,
			VIRTUALDESK = 0x4000u,
			WHEEL = 0x800u,
			XDOWN = 0x80u,
			XUP = 0x100u
		}

		public struct KEYBDINPUT {
			public VirtualKeyShort wVk;

			public ScanCodeShort wScan;

			public KEYEVENTF dwFlags;

			public int time;

			public UIntPtr dwExtraInfo;
		}

		[Flags]
		public enum KEYEVENTF : uint {
			EXTENDEDKEY = 1u,
			KEYUP = 2u,
			SCANCODE = 8u,
			UNICODE = 4u
		}

		public enum VirtualKeyShort : short {
			LBUTTON = 1,
			RBUTTON = 2,
			CANCEL = 3,
			MBUTTON = 4,
			XBUTTON1 = 5,
			XBUTTON2 = 6,
			BACK = 8,
			TAB = 9,
			CLEAR = 12,
			RETURN = 13,
			SHIFT = 16,
			CONTROL = 17,
			MENU = 18,
			PAUSE = 19,
			CAPITAL = 20,
			KANA = 21,
			HANGUL = 21,
			JUNJA = 23,
			FINAL = 24,
			HANJA = 25,
			KANJI = 25,
			ESCAPE = 27,
			CONVERT = 28,
			NONCONVERT = 29,
			ACCEPT = 30,
			MODECHANGE = 31,
			SPACE = 32,
			PRIOR = 33,
			NEXT = 34,
			END = 35,
			HOME = 36,
			LEFT = 37,
			UP = 38,
			RIGHT = 39,
			DOWN = 40,
			SELECT = 41,
			PRINT = 42,
			EXECUTE = 43,
			SNAPSHOT = 44,
			INSERT = 45,
			DELETE = 46,
			HELP = 47,
			KEY_0 = 48,
			KEY_1 = 49,
			KEY_2 = 50,
			KEY_3 = 51,
			KEY_4 = 52,
			KEY_5 = 53,
			KEY_6 = 54,
			KEY_7 = 55,
			KEY_8 = 56,
			KEY_9 = 57,
			KEY_A = 65,
			KEY_B = 66,
			KEY_C = 67,
			KEY_D = 68,
			KEY_E = 69,
			KEY_F = 70,
			KEY_G = 71,
			KEY_H = 72,
			KEY_I = 73,
			KEY_J = 74,
			KEY_K = 75,
			KEY_L = 76,
			KEY_M = 77,
			KEY_N = 78,
			KEY_O = 79,
			KEY_P = 80,
			KEY_Q = 81,
			KEY_R = 82,
			KEY_S = 83,
			KEY_T = 84,
			KEY_U = 85,
			KEY_V = 86,
			KEY_W = 87,
			KEY_X = 88,
			KEY_Y = 89,
			KEY_Z = 90,
			LWIN = 91,
			RWIN = 92,
			APPS = 93,
			SLEEP = 95,
			NUMPAD0 = 96,
			NUMPAD1 = 97,
			NUMPAD2 = 98,
			NUMPAD3 = 99,
			NUMPAD4 = 100,
			NUMPAD5 = 101,
			NUMPAD6 = 102,
			NUMPAD7 = 103,
			NUMPAD8 = 104,
			NUMPAD9 = 105,
			MULTIPLY = 106,
			ADD = 107,
			SEPARATOR = 108,
			SUBTRACT = 109,
			DECIMAL = 110,
			DIVIDE = 111,
			F1 = 112,
			F2 = 113,
			F3 = 114,
			F4 = 115,
			F5 = 116,
			F6 = 117,
			F7 = 118,
			F8 = 119,
			F9 = 120,
			F10 = 121,
			F11 = 122,
			F12 = 123,
			F13 = 124,
			F14 = 125,
			F15 = 126,
			F16 = 127,
			F17 = 128,
			F18 = 129,
			F19 = 130,
			F20 = 131,
			F21 = 132,
			F22 = 133,
			F23 = 134,
			F24 = 135,
			NUMLOCK = 144,
			SCROLL = 145,
			LSHIFT = 160,
			RSHIFT = 161,
			LCONTROL = 162,
			RCONTROL = 163,
			LMENU = 164,
			RMENU = 165,
			BROWSER_BACK = 166,
			BROWSER_FORWARD = 167,
			BROWSER_REFRESH = 168,
			BROWSER_STOP = 169,
			BROWSER_SEARCH = 170,
			BROWSER_FAVORITES = 171,
			BROWSER_HOME = 172,
			VOLUME_MUTE = 173,
			VOLUME_DOWN = 174,
			VOLUME_UP = 175,
			MEDIA_NEXT_TRACK = 176,
			MEDIA_PREV_TRACK = 177,
			MEDIA_STOP = 178,
			MEDIA_PLAY_PAUSE = 179,
			LAUNCH_MAIL = 180,
			LAUNCH_MEDIA_SELECT = 181,
			LAUNCH_APP1 = 182,
			LAUNCH_APP2 = 183,
			OEM_1 = 186,
			OEM_PLUS = 187,
			OEM_COMMA = 188,
			OEM_MINUS = 189,
			OEM_PERIOD = 190,
			OEM_2 = 191,
			OEM_3 = 192,
			OEM_4 = 219,
			OEM_5 = 220,
			OEM_6 = 221,
			OEM_7 = 222,
			OEM_8 = 223,
			OEM_102 = 226,
			PROCESSKEY = 229,
			PACKET = 231,
			ATTN = 246,
			CRSEL = 247,
			EXSEL = 248,
			EREOF = 249,
			PLAY = 250,
			ZOOM = 251,
			NONAME = 252,
			PA1 = 253,
			OEM_CLEAR = 254
		}

		public enum ScanCodeShort : short {
			LBUTTON = 0,
			RBUTTON = 0,
			CANCEL = 70,
			MBUTTON = 0,
			XBUTTON1 = 0,
			XBUTTON2 = 0,
			BACK = 14,
			TAB = 15,
			CLEAR = 76,
			RETURN = 28,
			SHIFT = 42,
			CONTROL = 29,
			MENU = 56,
			PAUSE = 0,
			CAPITAL = 58,
			KANA = 0,
			HANGUL = 0,
			JUNJA = 0,
			FINAL = 0,
			HANJA = 0,
			KANJI = 0,
			ESCAPE = 1,
			CONVERT = 0,
			NONCONVERT = 0,
			ACCEPT = 0,
			MODECHANGE = 0,
			SPACE = 57,
			PRIOR = 73,
			NEXT = 81,
			END = 79,
			HOME = 71,
			LEFT = 75,
			UP = 72,
			RIGHT = 77,
			DOWN = 80,
			SELECT = 0,
			PRINT = 0,
			EXECUTE = 0,
			SNAPSHOT = 84,
			INSERT = 82,
			DELETE = 83,
			HELP = 99,
			KEY_0 = 11,
			KEY_1 = 2,
			KEY_2 = 3,
			KEY_3 = 4,
			KEY_4 = 5,
			KEY_5 = 6,
			KEY_6 = 7,
			KEY_7 = 8,
			KEY_8 = 9,
			KEY_9 = 10,
			KEY_A = 30,
			KEY_B = 48,
			KEY_C = 46,
			KEY_D = 32,
			KEY_E = 18,
			KEY_F = 33,
			KEY_G = 34,
			KEY_H = 35,
			KEY_I = 23,
			KEY_J = 36,
			KEY_K = 37,
			KEY_L = 38,
			KEY_M = 50,
			KEY_N = 49,
			KEY_O = 24,
			KEY_P = 25,
			KEY_Q = 16,
			KEY_R = 19,
			KEY_S = 31,
			KEY_T = 20,
			KEY_U = 22,
			KEY_V = 47,
			KEY_W = 17,
			KEY_X = 45,
			KEY_Y = 21,
			KEY_Z = 44,
			LWIN = 91,
			RWIN = 92,
			APPS = 93,
			SLEEP = 95,
			NUMPAD0 = 82,
			NUMPAD1 = 79,
			NUMPAD2 = 80,
			NUMPAD3 = 81,
			NUMPAD4 = 75,
			NUMPAD5 = 76,
			NUMPAD6 = 77,
			NUMPAD7 = 71,
			NUMPAD8 = 72,
			NUMPAD9 = 73,
			MULTIPLY = 55,
			ADD = 78,
			SEPARATOR = 0,
			SUBTRACT = 74,
			DECIMAL = 83,
			DIVIDE = 53,
			F1 = 59,
			F2 = 60,
			F3 = 61,
			F4 = 62,
			F5 = 63,
			F6 = 64,
			F7 = 65,
			F8 = 66,
			F9 = 67,
			F10 = 68,
			F11 = 87,
			F12 = 88,
			F13 = 100,
			F14 = 101,
			F15 = 102,
			F16 = 103,
			F17 = 104,
			F18 = 105,
			F19 = 106,
			F20 = 107,
			F21 = 108,
			F22 = 109,
			F23 = 110,
			F24 = 118,
			NUMLOCK = 69,
			SCROLL = 70,
			LSHIFT = 42,
			RSHIFT = 54,
			LCONTROL = 29,
			RCONTROL = 29,
			LMENU = 56,
			RMENU = 56,
			BROWSER_BACK = 106,
			BROWSER_FORWARD = 105,
			BROWSER_REFRESH = 103,
			BROWSER_STOP = 104,
			BROWSER_SEARCH = 101,
			BROWSER_FAVORITES = 102,
			BROWSER_HOME = 50,
			VOLUME_MUTE = 32,
			VOLUME_DOWN = 46,
			VOLUME_UP = 48,
			MEDIA_NEXT_TRACK = 25,
			MEDIA_PREV_TRACK = 16,
			MEDIA_STOP = 36,
			MEDIA_PLAY_PAUSE = 34,
			LAUNCH_MAIL = 108,
			LAUNCH_MEDIA_SELECT = 109,
			LAUNCH_APP1 = 107,
			LAUNCH_APP2 = 33,
			OEM_1 = 39,
			OEM_PLUS = 13,
			OEM_COMMA = 51,
			OEM_MINUS = 12,
			OEM_PERIOD = 52,
			OEM_2 = 53,
			OEM_3 = 41,
			OEM_4 = 26,
			OEM_5 = 43,
			OEM_6 = 27,
			OEM_7 = 40,
			OEM_8 = 0,
			OEM_102 = 86,
			PROCESSKEY = 0,
			PACKET = 0,
			ATTN = 0,
			CRSEL = 0,
			EXSEL = 0,
			EREOF = 93,
			PLAY = 0,
			ZOOM = 98,
			NONAME = 0,
			PA1 = 0,
			OEM_CLEAR = 0
		}

		public const uint INPUT_KEYBOARD = 1u;

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool TrackMouseEvent(ref Structs.TRACKMOUSEEVENT lpEventTrack);

		[DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(int vkey);

		[DllImport("user32.dll")]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("user32.dll")]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr handle, int wMsg, int wParam, int lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindow(IntPtr handle, uint wCmd);

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr handle);

		[DllImport("user32.dll")]
		public static extern bool GetCaretPos(out Point pt);

		[DllImport("user32.dll")]
		public static extern bool SetCaretPos(int x, int y);

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern int GetDlgCtrlID(IntPtr hwndCtl);

		[DllImport("user32.dll")]
		public static extern int ScrollWindowEx(IntPtr hWnd, int dx, int dy, IntPtr prcScroll, IntPtr prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, uint flags);

		[DllImport("user32.dll")]
		public static extern uint SendInput(uint nInputs, [In][MarshalAs(UnmanagedType.LPArray)] INPUT[] pInputs, int cbSize);
	}
}

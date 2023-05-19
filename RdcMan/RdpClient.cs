using System;
using System.Drawing;
using System.Windows.Forms;
using AxMSTSCLib;
using MSTSCLib;

namespace RdcMan
{
	public class RdpClient
	{
		public enum ConnectionState
		{
			Disconnected,
			Connecting,
			Connected
		}

		public enum ConnectionBarState
		{
			AutoHide,
			Pinned,
			Off
		}

		public enum AudioRedirectionMode
		{
			Client,
			Remote,
			NoSound
		}

		public enum AudioRedirectionQuality
		{
			Dynamic,
			High,
			Medium
		}

		public enum AudioCaptureRedirectionMode
		{
			DoNotRecord,
			Record
		}

		public enum KeyboardHookMode
		{
			Client,
			Remote,
			FullScreenClient
		}

		public enum GatewayUsageMethod
		{
			NoneDirect,
			ProxyDirect,
			ProxyDetect,
			Default,
			NoneDetect
		}

		public enum GatewayLogonMethod
		{
			NTLM = 0,
			SmartCard = 1,
			Any = 4
		}

		public enum AuthenticationLevel
		{
			None,
			Required,
			Warn
		}

		public enum SmartSizeMethod
		{
			None,
			Scale,
			Reconnect
		}

		public delegate void VoidDelegate();

		public const int DefaultRDPPort = 3389;

		public const int DefaultVMConsoleConnectPort = 2179;

		public const int DefaultColorDepth = 24;

		public const int PerfDisableNothing = 0;

		public const int PerfDisableWallpaper = 1;

		public const int PerfDisableFullWindowDrag = 2;

		public const int PerfDisableMenuAnimations = 4;

		public const int PerfDisableTheming = 8;

		public const int PerfEnableEnhancedGraphics = 16;

		public const int PerfDisableCursorShadow = 32;

		public const int PerfDisableCursorBlinking = 64;

		public const int PerfEnableFontSmoothing = 128;

		public const int PerfEnableDesktopComposition = 256;

		public static bool SupportsGatewayCredentials;

		public static bool SupportsAdvancedAudioVideoRedirection;

		public static bool SupportsMonitorSpanning;

		public static bool SupportsPanning;

		public static bool SupportsFineGrainedRedirection;

		public static bool SupportsRemoteSessionActions;

		private Server _server;

		public static int MaxDesktopHeight;

		public static int MaxDesktopWidth;

		public static string RdpControlVersion;

		private static int RdpClientVersion;

		private static RdpClient StaticClient;

		private RdpClient5 _rdpClient5;

		private RdpClient6 _rdpClient6;

		private RdpClient7 _rdpClient7;

		private RdpClient8 _rdpClient8;

		private RdpClient9 _rdpClient9;

		private RdpClient10 _rdpClient10;

		public Size DesktopSize
		{
			get
			{
				return new Size(MsRdpClient.DesktopWidth, MsRdpClient.DesktopHeight);
			}
			set
			{
				MsRdpClient.DesktopHeight = Math.Min(MaxDesktopHeight, value.Height);
				MsRdpClient.DesktopWidth = Math.Min(MaxDesktopWidth, value.Width);
			}
		}

		public Control Control
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6;
				}
				return _rdpClient5;
			}
		}

		public IMsRdpClient MsRdpClient => GetOcx() as IMsRdpClient;

		public IMsRdpClientAdvancedSettings AdvancedSettings2
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.AdvancedSettings2;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.AdvancedSettings2;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.AdvancedSettings2;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.AdvancedSettings2;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6.AdvancedSettings2;
				}
				return _rdpClient5.AdvancedSettings2;
			}
		}

		public IMsRdpClientAdvancedSettings4 AdvancedSettings5
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.AdvancedSettings5;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.AdvancedSettings5;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.AdvancedSettings5;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.AdvancedSettings5;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6.AdvancedSettings5;
				}
				return _rdpClient5.AdvancedSettings5;
			}
		}

		public IMsRdpClientAdvancedSettings5 AdvancedSettings6
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.AdvancedSettings6;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.AdvancedSettings6;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.AdvancedSettings6;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.AdvancedSettings6;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6.AdvancedSettings6;
				}
				return _rdpClient5.AdvancedSettings6;
			}
		}

		public IMsRdpClientAdvancedSettings6 AdvancedSettings7
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.AdvancedSettings7;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.AdvancedSettings7;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.AdvancedSettings7;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.AdvancedSettings7;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6.AdvancedSettings7;
				}
				return null;
			}
		}

		public IMsRdpClientAdvancedSettings7 AdvancedSettings8
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.AdvancedSettings8;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.AdvancedSettings8;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.AdvancedSettings8;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.AdvancedSettings8;
				}
				return null;
			}
		}

		public IMsRdpClientNonScriptable3 ClientNonScriptable3 => GetOcx() as IMsRdpClientNonScriptable3;

		public IMsRdpClient8 MsRdpClient8 => MsRdpClient as IMsRdpClient8;

		public IMsRdpClientTransportSettings TransportSettings
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.TransportSettings;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.TransportSettings;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.TransportSettings;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.TransportSettings;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6.TransportSettings;
				}
				return _rdpClient5.TransportSettings;
			}
		}

		public IMsRdpClientTransportSettings2 TransportSettings2
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.TransportSettings2;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.TransportSettings2;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.TransportSettings2;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.TransportSettings2;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6.TransportSettings2;
				}
				return null;
			}
		}

		public IMsTscSecuredSettings SecuredSettings
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.SecuredSettings;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.SecuredSettings;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.SecuredSettings;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.SecuredSettings;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6.SecuredSettings;
				}
				return _rdpClient5.SecuredSettings;
			}
		}

		public IMsRdpClientSecuredSettings SecuredSettings2
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.SecuredSettings2;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.SecuredSettings2;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.SecuredSettings2;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.SecuredSettings2;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6.SecuredSettings2;
				}
				return _rdpClient5.SecuredSettings2;
			}
		}

		public ITSRemoteProgram RemoteProgram
		{
			get
			{
				if (_rdpClient10 != null)
				{
					return _rdpClient10.RemoteProgram;
				}
				if (_rdpClient9 != null)
				{
					return _rdpClient9.RemoteProgram;
				}
				if (_rdpClient8 != null)
				{
					return _rdpClient8.RemoteProgram;
				}
				if (_rdpClient7 != null)
				{
					return _rdpClient7.RemoteProgram;
				}
				if (_rdpClient6 != null)
				{
					return _rdpClient6.RemoteProgram;
				}
				return _rdpClient5.RemoteProgram;
			}
		}

		public static IMsRdpDriveCollection DriveCollection => StaticClient.ClientNonScriptable3.DriveCollection;

		public static IMsRdpDeviceCollection DeviceCollection => StaticClient.ClientNonScriptable3.DeviceCollection;

		public static string AudioRedirectionModeToString(AudioRedirectionMode mode) {
			return mode switch {
				AudioRedirectionMode.Client => "������̨������",
				AudioRedirectionMode.Remote => "����Զ�˵�����",
				AudioRedirectionMode.NoSound => "������",
				_ => throw new Exception("�������Ƶ�ض���ģʽ��" + mode),
			};
		}

		public static string AudioRedirectionQualityToString(AudioRedirectionQuality mode) {
			return mode switch {
				AudioRedirectionQuality.Dynamic => "��̬����",
				AudioRedirectionQuality.High => "������",
				AudioRedirectionQuality.Medium => "�е�����",
				_ => throw new Exception("�������Ƶ�ض���������" + mode),
			};
		}

		public static string AudioCaptureRedirectionModeToString(AudioCaptureRedirectionMode mode) {
			return mode switch {
				AudioCaptureRedirectionMode.DoNotRecord => "��¼��",
				AudioCaptureRedirectionMode.Record => "����̨����¼��",
				_ => throw new Exception("����� ��Ƶ�����ض���ģʽ��" + mode),
			};
		}

		public static string KeyboardHookModeToString(KeyboardHookMode mode) {
			return mode switch {
				KeyboardHookMode.Client => "�ڱ��ؼ������",
				KeyboardHookMode.Remote => "��Զ�̼������",
				KeyboardHookMode.FullScreenClient => "����ȫ��ģʽ��",
				_ => throw new Exception("����ļ��̹���ģʽ��" + mode),
			};
		}

		public static string GatewayLogonMethodToString(GatewayLogonMethod mode) {
			return mode switch {
				GatewayLogonMethod.NTLM => "ѯ������ (NTLM)",
				GatewayLogonMethod.SmartCard => "���ܿ�",
				GatewayLogonMethod.Any => "�������Ժ�ѡ��",
				_ => null,
			};
		}

		public static string GatewayUsageMethodToString(GatewayUsageMethod mode) {
			return mode switch {
				GatewayUsageMethod.NoneDetect => "�Զ��������",
				GatewayUsageMethod.NoneDirect => "��ʹ�����ط�����",
				_ => throw new Exception("���������ʹ�÷�ʽ��" + mode),
			};
		}

		public static string AuthenticationLevelToString(AuthenticationLevel mode) {
			return mode switch {
				AuthenticationLevel.None => "�����֤ʧ��ʱ�������Ӳ��Ҳ���������",
				AuthenticationLevel.Warn => "��֤ʧ��ʱ��������",
				AuthenticationLevel.Required => "�����֤ʧʱ������",
				_ => throw new Exception("����������֤����" + mode),
			};
		}

		public static string SmartSizeMethodToString(SmartSizeMethod method) {
			return method switch {
				SmartSizeMethod.None => "������",
				SmartSizeMethod.Scale => "�������ʺϴ���",
				SmartSizeMethod.Reconnect => "���·ֱ�����������",
				_ => throw new Exception("������Զ��ߴ緽ʽ��" + method),
			};
		}

		private RdpClient(MainForm form)
		{
			switch (RdpClientVersion)
			{
			case 10:
				_rdpClient10 = new RdpClient10(form);
				break;
			case 9:
				_rdpClient9 = new RdpClient9(form);
				break;
			case 8:
				_rdpClient8 = new RdpClient8(form);
				break;
			case 7:
				_rdpClient7 = new RdpClient7(form);
				break;
			case 6:
				_rdpClient6 = new RdpClient6(form);
				break;
			default:
				_rdpClient5 = new RdpClient5(form);
				break;
			}
		}

		internal static void Initialize(MainForm form)
		{
			using (RdpClient5 rdpClient = new RdpClient5(form))
			{
				RdpControlVersion = rdpClient.Version;
				string[] array = rdpClient.Version.Split('.');
				int num = int.Parse(array[2]);
				if (num >= 10240)
				{
					RdpClientVersion = 9;
				}
				else if (num >= 9600)
				{
					RdpClientVersion = 9;
				}
				else if (num >= 9200)
				{
					RdpClientVersion = 8;
				}
				else if (num >= 7600)
				{
					RdpClientVersion = 7;
				}
				else if (num >= 6001)
				{
					RdpClientVersion = 6;
				}
				else
				{
					RdpClientVersion = 5;
				}
				if (RdpClientVersion >= 6)
				{
					SupportsMonitorSpanning = true;
				}
				form.RemoveFromClientPanel(rdpClient);
			}
			StaticClient = new RdpClient(form);
			RdpClient staticClient = StaticClient;
			staticClient.Control.Enabled = false;
			MaxDesktopWidth = 4096;
			MaxDesktopHeight = 2048;
			if (staticClient.AdvancedSettings7 != null)
			{
				SupportsGatewayCredentials = true;
			}
			if (staticClient.AdvancedSettings8 != null)
			{
				SupportsAdvancedAudioVideoRedirection = true;
			}
			if (staticClient.ClientNonScriptable3 != null)
			{
				SupportsFineGrainedRedirection = true;
			}
			if (staticClient.MsRdpClient8 != null)
			{
				SupportsRemoteSessionActions = true;
			}
		}

		internal static RdpClient AllocClient(Server server, MainForm form)
		{
			RdpClient rdpClient = new RdpClient(form);
			rdpClient._server = server;
			return rdpClient;
		}

		internal static void ReleaseClient(RdpClient client)
		{
			try
			{
				client._server = null;
				Program.TheForm.RemoveFromClientPanel(client.Control);
			}
			finally
			{
				AxHost rdpClient = client._rdpClient5;
				if (rdpClient != null)
				{
					client._rdpClient5 = null;
					rdpClient.Dispose();
				}
				rdpClient = client._rdpClient6;
				if (rdpClient != null)
				{
					client._rdpClient6 = null;
					rdpClient.Dispose();
				}
				rdpClient = client._rdpClient7;
				if (rdpClient != null)
				{
					client._rdpClient7 = null;
					rdpClient.Dispose();
				}
				rdpClient = client._rdpClient8;
				if (rdpClient != null)
				{
					client._rdpClient8 = null;
					rdpClient.Dispose();
				}
				rdpClient = client._rdpClient9;
				if (rdpClient != null)
				{
					client._rdpClient9 = null;
					rdpClient.Dispose();
				}
				rdpClient = client._rdpClient10;
				if (rdpClient != null)
				{
					client._rdpClient10 = null;
					rdpClient.Dispose();
				}
			}
		}

		public void SetText()
		{
			if (_rdpClient10 != null)
			{
				_rdpClient10.ConnectingText = _server.ConnectingText;
				_rdpClient10.DisconnectedText = _server.DisconnectedText;
			}
			else if (_rdpClient9 != null)
			{
				_rdpClient9.ConnectingText = _server.ConnectingText;
				_rdpClient9.DisconnectedText = _server.DisconnectedText;
			}
			else if (_rdpClient8 != null)
			{
				_rdpClient8.ConnectingText = _server.ConnectingText;
				_rdpClient8.DisconnectedText = _server.DisconnectedText;
			}
			else if (_rdpClient7 != null)
			{
				_rdpClient7.ConnectingText = _server.ConnectingText;
				_rdpClient7.DisconnectedText = _server.DisconnectedText;
			}
			else if (_rdpClient6 != null)
			{
				_rdpClient6.ConnectingText = _server.ConnectingText;
				_rdpClient6.DisconnectedText = _server.DisconnectedText;
			}
			else
			{
				_rdpClient5.ConnectingText = _server.ConnectingText;
				_rdpClient5.DisconnectedText = _server.DisconnectedText;
			}
		}

		public object GetOcx()
		{
			if (_rdpClient10 != null)
			{
				return _rdpClient10.GetOcx();
			}
			if (_rdpClient9 != null)
			{
				return _rdpClient9.GetOcx();
			}
			if (_rdpClient8 != null)
			{
				return _rdpClient8.GetOcx();
			}
			if (_rdpClient7 != null)
			{
				return _rdpClient7.GetOcx();
			}
			if (_rdpClient6 != null)
			{
				return _rdpClient6.GetOcx();
			}
			return _rdpClient5.GetOcx();
		}

		public void ConnectConnectionHandlers(EventHandler onConnected, EventHandler onConnecting, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEventHandler onDisconnected, AxMSTSCLib.IMsTscAxEvents_OnAutoReconnectingEventHandler onAutoReconnecting, AxMSTSCLib.IMsTscAxEvents_OnAutoReconnecting2EventHandler onAutoReconnecting2, EventHandler onAutoReconnected, AxMSTSCLib.IMsTscAxEvents_OnFocusReleasedEventHandler onFocusReleased)
		{
			if (_rdpClient10 != null)
			{
				_rdpClient10.OnConnected += onConnected;
				_rdpClient10.OnConnecting += onConnecting;
				_rdpClient10.OnDisconnected += onDisconnected;
				_rdpClient10.OnAutoReconnecting += onAutoReconnecting;
				_rdpClient10.OnAutoReconnecting2 += onAutoReconnecting2;
				_rdpClient10.OnAutoReconnected += onAutoReconnected;
				_rdpClient10.OnFocusReleased += onFocusReleased;
			}
			else if (_rdpClient9 != null)
			{
				_rdpClient9.OnConnected += onConnected;
				_rdpClient9.OnConnecting += onConnecting;
				_rdpClient9.OnDisconnected += onDisconnected;
				_rdpClient9.OnAutoReconnecting += onAutoReconnecting;
				_rdpClient9.OnAutoReconnecting2 += onAutoReconnecting2;
				_rdpClient9.OnAutoReconnected += onAutoReconnected;
				_rdpClient9.OnFocusReleased += onFocusReleased;
			}
			else if (_rdpClient8 != null)
			{
				_rdpClient8.OnConnected += onConnected;
				_rdpClient8.OnConnecting += onConnecting;
				_rdpClient8.OnDisconnected += onDisconnected;
				_rdpClient8.OnAutoReconnecting += onAutoReconnecting;
				_rdpClient8.OnAutoReconnecting2 += onAutoReconnecting2;
				_rdpClient8.OnAutoReconnected += onAutoReconnected;
				_rdpClient8.OnFocusReleased += onFocusReleased;
			}
			else if (_rdpClient7 != null)
			{
				_rdpClient7.OnConnected += onConnected;
				_rdpClient7.OnConnecting += onConnecting;
				_rdpClient7.OnDisconnected += onDisconnected;
				_rdpClient7.OnAutoReconnecting += onAutoReconnecting;
				_rdpClient7.OnAutoReconnecting2 += onAutoReconnecting2;
				_rdpClient7.OnAutoReconnected += onAutoReconnected;
				_rdpClient7.OnFocusReleased += onFocusReleased;
			}
			else if (_rdpClient6 != null)
			{
				_rdpClient6.OnConnected += onConnected;
				_rdpClient6.OnConnecting += onConnecting;
				_rdpClient6.OnDisconnected += onDisconnected;
				_rdpClient6.OnAutoReconnecting += onAutoReconnecting;
				_rdpClient6.OnAutoReconnecting2 += onAutoReconnecting2;
				_rdpClient6.OnAutoReconnected += onAutoReconnected;
				_rdpClient6.OnFocusReleased += onFocusReleased;
			}
			else
			{
				_rdpClient5.OnConnected += onConnected;
				_rdpClient5.OnConnecting += onConnecting;
				_rdpClient5.OnDisconnected += onDisconnected;
				_rdpClient5.OnAutoReconnecting += onAutoReconnecting;
				_rdpClient5.OnAutoReconnecting2 += onAutoReconnecting2;
				_rdpClient5.OnAutoReconnected += onAutoReconnected;
				_rdpClient5.OnFocusReleased += onFocusReleased;
			}
		}

		public void DisconnectConnectionHandlers(EventHandler onConnected, EventHandler onConnecting, AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEventHandler onDisconnected, AxMSTSCLib.IMsTscAxEvents_OnAutoReconnectingEventHandler onAutoReconnecting, AxMSTSCLib.IMsTscAxEvents_OnAutoReconnecting2EventHandler onAutoReconnecting2, EventHandler onAutoReconnected, AxMSTSCLib.IMsTscAxEvents_OnFocusReleasedEventHandler onFocusReleased)
		{
			if (_rdpClient10 != null)
			{
				_rdpClient10.OnConnected -= onConnected;
				_rdpClient10.OnConnecting -= onConnecting;
				_rdpClient10.OnDisconnected -= onDisconnected;
				_rdpClient10.OnAutoReconnecting -= onAutoReconnecting;
				_rdpClient10.OnAutoReconnecting2 -= onAutoReconnecting2;
				_rdpClient10.OnAutoReconnected -= onAutoReconnected;
				_rdpClient10.OnFocusReleased -= onFocusReleased;
			}
			else if (_rdpClient9 != null)
			{
				_rdpClient9.OnConnected -= onConnected;
				_rdpClient9.OnConnecting -= onConnecting;
				_rdpClient9.OnDisconnected -= onDisconnected;
				_rdpClient9.OnAutoReconnecting -= onAutoReconnecting;
				_rdpClient9.OnAutoReconnecting2 -= onAutoReconnecting2;
				_rdpClient9.OnAutoReconnected -= onAutoReconnected;
				_rdpClient9.OnFocusReleased -= onFocusReleased;
			}
			else if (_rdpClient8 != null)
			{
				_rdpClient8.OnConnected -= onConnected;
				_rdpClient8.OnConnecting -= onConnecting;
				_rdpClient8.OnDisconnected -= onDisconnected;
				_rdpClient8.OnAutoReconnecting -= onAutoReconnecting;
				_rdpClient8.OnAutoReconnecting2 -= onAutoReconnecting2;
				_rdpClient8.OnAutoReconnected -= onAutoReconnected;
				_rdpClient8.OnFocusReleased -= onFocusReleased;
			}
			else if (_rdpClient7 != null)
			{
				_rdpClient7.OnConnected -= onConnected;
				_rdpClient7.OnConnecting -= onConnecting;
				_rdpClient7.OnDisconnected -= onDisconnected;
				_rdpClient7.OnAutoReconnecting -= onAutoReconnecting;
				_rdpClient7.OnAutoReconnecting2 -= onAutoReconnecting2;
				_rdpClient7.OnAutoReconnected -= onAutoReconnected;
				_rdpClient7.OnFocusReleased -= onFocusReleased;
			}
			else if (_rdpClient6 != null)
			{
				_rdpClient6.OnConnected -= onConnected;
				_rdpClient6.OnConnecting -= onConnecting;
				_rdpClient6.OnDisconnected -= onDisconnected;
				_rdpClient6.OnAutoReconnecting -= onAutoReconnecting;
				_rdpClient6.OnAutoReconnecting2 -= onAutoReconnecting2;
				_rdpClient6.OnAutoReconnected -= onAutoReconnected;
				_rdpClient6.OnFocusReleased -= onFocusReleased;
			}
			else
			{
				_rdpClient5.OnConnected -= onConnected;
				_rdpClient5.OnConnecting -= onConnecting;
				_rdpClient5.OnDisconnected -= onDisconnected;
				_rdpClient5.OnAutoReconnecting -= onAutoReconnecting;
				_rdpClient5.OnAutoReconnecting2 -= onAutoReconnecting2;
				_rdpClient5.OnAutoReconnected -= onAutoReconnected;
				_rdpClient5.OnFocusReleased -= onFocusReleased;
			}
		}

		public void ConnectContainerHandlers(EventHandler onRequestGoFullScreen, EventHandler onRequestLeaveFullScreen, EventHandler onRequestContainerMinimize, AxMSTSCLib.IMsTscAxEvents_OnConfirmCloseEventHandler onConfirmClose, AxMSTSCLib.IMsTscAxEvents_OnFatalErrorEventHandler onFatalError)
		{
			if (_rdpClient10 != null)
			{
				_rdpClient10.OnRequestGoFullScreen += onRequestGoFullScreen;
				_rdpClient10.OnRequestLeaveFullScreen += onRequestLeaveFullScreen;
				_rdpClient10.OnRequestContainerMinimize += onRequestContainerMinimize;
				_rdpClient10.OnConfirmClose += onConfirmClose;
				_rdpClient10.OnFatalError += onFatalError;
			}
			else if (_rdpClient9 != null)
			{
				_rdpClient9.OnRequestGoFullScreen += onRequestGoFullScreen;
				_rdpClient9.OnRequestLeaveFullScreen += onRequestLeaveFullScreen;
				_rdpClient9.OnRequestContainerMinimize += onRequestContainerMinimize;
				_rdpClient9.OnConfirmClose += onConfirmClose;
				_rdpClient9.OnFatalError += onFatalError;
			}
			else if (_rdpClient8 != null)
			{
				_rdpClient8.OnRequestGoFullScreen += onRequestGoFullScreen;
				_rdpClient8.OnRequestLeaveFullScreen += onRequestLeaveFullScreen;
				_rdpClient8.OnRequestContainerMinimize += onRequestContainerMinimize;
				_rdpClient8.OnConfirmClose += onConfirmClose;
				_rdpClient8.OnFatalError += onFatalError;
			}
			else if (_rdpClient7 != null)
			{
				_rdpClient7.OnRequestGoFullScreen += onRequestGoFullScreen;
				_rdpClient7.OnRequestLeaveFullScreen += onRequestLeaveFullScreen;
				_rdpClient7.OnRequestContainerMinimize += onRequestContainerMinimize;
				_rdpClient7.OnConfirmClose += onConfirmClose;
				_rdpClient7.OnFatalError += onFatalError;
			}
			else if (_rdpClient6 != null)
			{
				_rdpClient6.OnRequestGoFullScreen += onRequestGoFullScreen;
				_rdpClient6.OnRequestLeaveFullScreen += onRequestLeaveFullScreen;
				_rdpClient6.OnRequestContainerMinimize += onRequestContainerMinimize;
				_rdpClient6.OnConfirmClose += onConfirmClose;
				_rdpClient6.OnFatalError += onFatalError;
			}
			else
			{
				_rdpClient5.OnRequestGoFullScreen += onRequestGoFullScreen;
				_rdpClient5.OnRequestLeaveFullScreen += onRequestLeaveFullScreen;
				_rdpClient5.OnRequestContainerMinimize += onRequestContainerMinimize;
				_rdpClient5.OnConfirmClose += onConfirmClose;
				_rdpClient5.OnFatalError += onFatalError;
			}
		}

		public void DisconnectContainerHandlers(EventHandler onRequestGoFullScreen, EventHandler onRequestLeaveFullScreen, EventHandler onRequestContainerMinimize, AxMSTSCLib.IMsTscAxEvents_OnConfirmCloseEventHandler onConfirmClose, AxMSTSCLib.IMsTscAxEvents_OnFatalErrorEventHandler onFatalError)
		{
			if (_rdpClient10 != null)
			{
				_rdpClient10.OnRequestGoFullScreen -= onRequestGoFullScreen;
				_rdpClient10.OnRequestLeaveFullScreen -= onRequestLeaveFullScreen;
				_rdpClient10.OnRequestContainerMinimize -= onRequestContainerMinimize;
				_rdpClient10.OnConfirmClose -= onConfirmClose;
				_rdpClient10.OnFatalError -= onFatalError;
			}
			else if (_rdpClient9 != null)
			{
				_rdpClient9.OnRequestGoFullScreen -= onRequestGoFullScreen;
				_rdpClient9.OnRequestLeaveFullScreen -= onRequestLeaveFullScreen;
				_rdpClient9.OnRequestContainerMinimize -= onRequestContainerMinimize;
				_rdpClient9.OnConfirmClose -= onConfirmClose;
				_rdpClient9.OnFatalError -= onFatalError;
			}
			else if (_rdpClient8 != null)
			{
				_rdpClient8.OnRequestGoFullScreen -= onRequestGoFullScreen;
				_rdpClient8.OnRequestLeaveFullScreen -= onRequestLeaveFullScreen;
				_rdpClient8.OnRequestContainerMinimize -= onRequestContainerMinimize;
				_rdpClient8.OnConfirmClose -= onConfirmClose;
				_rdpClient8.OnFatalError -= onFatalError;
			}
			else if (_rdpClient7 != null)
			{
				_rdpClient7.OnRequestGoFullScreen -= onRequestGoFullScreen;
				_rdpClient7.OnRequestLeaveFullScreen -= onRequestLeaveFullScreen;
				_rdpClient7.OnRequestContainerMinimize -= onRequestContainerMinimize;
				_rdpClient7.OnConfirmClose -= onConfirmClose;
				_rdpClient7.OnFatalError -= onFatalError;
			}
			else if (_rdpClient6 != null)
			{
				_rdpClient6.OnRequestGoFullScreen -= onRequestGoFullScreen;
				_rdpClient6.OnRequestLeaveFullScreen -= onRequestLeaveFullScreen;
				_rdpClient6.OnRequestContainerMinimize -= onRequestContainerMinimize;
				_rdpClient6.OnConfirmClose -= onConfirmClose;
				_rdpClient6.OnFatalError -= onFatalError;
			}
			else
			{
				_rdpClient5.OnRequestGoFullScreen -= onRequestGoFullScreen;
				_rdpClient5.OnRequestLeaveFullScreen -= onRequestLeaveFullScreen;
				_rdpClient5.OnRequestContainerMinimize -= onRequestContainerMinimize;
				_rdpClient5.OnConfirmClose -= onConfirmClose;
				_rdpClient5.OnFatalError -= onFatalError;
			}
		}

		public void Dump()
		{
			try
			{
				if (_rdpClient10 != null)
				{
					Log.DumpObject(_rdpClient10.AdvancedSettings9);
					Log.DumpObject(_rdpClient10.SecuredSettings3);
					Log.DumpObject(_rdpClient10.TransportSettings3);
					Log.DumpObject((IMsRdpClientNonScriptable5)_rdpClient10.GetOcx());
				}
				else if (_rdpClient9 != null)
				{
					Log.DumpObject(_rdpClient9.AdvancedSettings9);
					Log.DumpObject(_rdpClient9.SecuredSettings3);
					Log.DumpObject(_rdpClient9.TransportSettings3);
					Log.DumpObject((IMsRdpClientNonScriptable5)_rdpClient9.GetOcx());
				}
				else if (_rdpClient8 != null)
				{
					Log.DumpObject(_rdpClient8.AdvancedSettings9);
					Log.DumpObject(_rdpClient8.SecuredSettings3);
					Log.DumpObject(_rdpClient8.TransportSettings3);
					Log.DumpObject((IMsRdpClientNonScriptable5)_rdpClient8.GetOcx());
				}
				else if (_rdpClient7 != null)
				{
					Log.DumpObject(_rdpClient7.AdvancedSettings8);
					Log.DumpObject(_rdpClient7.SecuredSettings3);
					Log.DumpObject(_rdpClient7.TransportSettings3);
					Log.DumpObject((IMsRdpClientNonScriptable5)_rdpClient7.GetOcx());
				}
				else if (_rdpClient6 != null)
				{
					Log.DumpObject(_rdpClient6.AdvancedSettings7);
					Log.DumpObject(_rdpClient6.SecuredSettings2);
					Log.DumpObject(_rdpClient6.TransportSettings2);
					Log.DumpObject((IMsRdpClientNonScriptable4)_rdpClient6.GetOcx());
				}
				else if (_rdpClient5 != null)
				{
					Log.DumpObject(_rdpClient5.AdvancedSettings6);
					Log.DumpObject(_rdpClient5.SecuredSettings2);
					Log.DumpObject(_rdpClient5.TransportSettings);
					Log.DumpObject((IMsRdpClientNonScriptable4)_rdpClient5.GetOcx());
				}
			}
			catch
			{
			}
		}

		public bool Resize(uint width, uint height)
		{
			if (_rdpClient10 != null)
			{
				_rdpClient10.Reconnect(width, height);
				return true;
			}
			if (_rdpClient9 != null)
			{
				_rdpClient9.Reconnect(width, height);
				return true;
			}
			if (_rdpClient8 != null)
			{
				_rdpClient8.Reconnect(width, height);
				return true;
			}
			return false;
		}
	}
}

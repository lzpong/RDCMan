using System.Windows.Forms;
using MSTSCLib;
using Win32;

namespace RdcMan {
	internal class SendKeys {
		private struct SendKeysData {
			public const int MaxKeys = 20;

			public unsafe fixed short keyUp[20];

			public unsafe fixed int keyData[20];
		}

		public unsafe static void Send(Keys[] keyCodes, ServerBase serverBase) {
			Server serverNode = serverBase.ServerNode;
			RdpClient client = serverNode.Client;
			IMsRdpClientNonScriptable msRdpClientNonScriptable = (IMsRdpClientNonScriptable)client.GetOcx();
			int num = keyCodes.Length;
			try {
				SendKeysData sendKeysData = default(SendKeysData);
				bool* ptr = (bool*)sendKeysData.keyUp;
				int* ptr2 = sendKeysData.keyData;
				int num2 = 0;
				for (int i = 0; i < num && i < 10; i++) {
					int num3 = (int)Util.MapVirtualKey((uint)keyCodes[i], 0u);
					sendKeysData.keyData[num2] = num3;
					sendKeysData.keyUp[num2++] = 0;
					if (!IsModifier(keyCodes[i])) {
						for (int num4 = num2 - 1; num4 >= 0; num4--) {
							sendKeysData.keyData[num2] = sendKeysData.keyData[num4];
							sendKeysData.keyUp[num2++] = 1;
						}
						msRdpClientNonScriptable.SendKeys(num2, ref *ptr, ref *ptr2);
						num2 = 0;
					}
				}
			}
			catch { }
		}

		private static bool IsModifier(Keys key) {
			if ((uint)(key - 16) <= 2u || (uint)(key - 91) <= 1u || (uint)(key - 162) <= 1u)
				return true;

			return false;
		}
	}
}

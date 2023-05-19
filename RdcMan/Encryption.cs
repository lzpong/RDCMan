using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using Win32;

namespace RdcMan
{
	public static class Encryption
	{
		private static readonly List<DeferDecryptionItem> PendingDecryption = new List<DeferDecryptionItem>();

		public static void DeferDecryption(IDeferDecryption o, RdcTreeNode node, string errorDetail)
		{
			PendingDecryption.Add(new DeferDecryptionItem
			{
				Object = o,
				Node = node,
				ErrorDetail = errorDetail
			});
		}

		public static void DecryptPasswords()
		{
			StringBuilder builder = new StringBuilder("解密某些凭证时出现问题。").AppendLine().AppendLine("单击确定将详细信息复制到剪贴板。");
			bool credentialsProfileFail = false;
			PendingDecryption.Where((DeferDecryptionItem d) => d.Object is CredentialsProfile).ForEach(delegate(DeferDecryptionItem item)
			{
				DecryptPassword(item, builder, "凭证配置文件：", ref credentialsProfileFail);
			});
			bool passwordFail = false;
			PendingDecryption.Where((DeferDecryptionItem d) => d.Object is PasswordSetting).ForEach(delegate(DeferDecryptionItem item)
			{
				DecryptPassword(item, builder, "自定义密码：", ref passwordFail);
			});
			PendingDecryption.Clear();
			if (credentialsProfileFail || passwordFail)
			{
				DialogResult dialogResult = FormTools.ExclamationDialog(builder.ToString(), MessageBoxButtons.OKCancel);
				if (dialogResult == DialogResult.OK)
				{
					Clipboard.SetText(builder.ToString());
				}
			}
		}

		private static bool DecryptPassword(DeferDecryptionItem item, StringBuilder builder, string header, ref bool anyFailed)
		{
			IDeferDecryption @object = item.Object;
			RdcTreeNode node = item.Node;
			string errorDetail = item.ErrorDetail;
			bool anyInherited = false;
			node.EncryptionSettings.InheritSettings(node, ref anyInherited);
			try
			{
				@object.Decrypt(node.EncryptionSettings);
			}
			catch (Exception ex)
			{
				if (!anyFailed)
				{
					builder.AppendLine().AppendLine(header);
					anyFailed = true;
				}
				if (node is DefaultSettingsGroup)
				{
					builder.Append("默认设置");
				}
				else
				{
					builder.Append(node.FullPath);
				}
				builder.AppendFormat(": {0}", errorDetail);
				if (!string.IsNullOrEmpty(ex.Message))
				{
					builder.AppendFormat(" [{0}]", ex.Message);
				}
				builder.AppendLine();
			}
			return anyFailed;
		}

		public static string SimpleName(this X509Certificate2 cert)
		{
			string text = cert.FriendlyName;
			if (string.IsNullOrEmpty(text))
			{
				text = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
			}
			return text + ", " + cert.GetNameInfo(X509NameType.SimpleName, forIssuer: true);
		}

		public static string EncryptionMethodToString(EncryptionMethod method)
		{
			return method switch
			{
				EncryptionMethod.Certificate => "证书",
				EncryptionMethod.LogonCredentials => "使用用户的凭证登录",
				_ => throw new Exception("意外的加密方法"),
			};
		}

		public static X509Certificate2 SelectCertificate()
		{
			X509Store x509Store = new X509Store();
			X509Certificate2Collection privateCollection = new X509Certificate2Collection();
			try
			{
				x509Store.Open(OpenFlags.OpenExistingOnly);
				X509Certificate2Collection certificates = x509Store.Certificates;
				X509Certificate2Collection foundCollection = certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, validOnly: false);
				LongRunningActionForm.PerformOperation("检查有效证书", showImmediately: true, delegate
				{
					X509Certificate2Enumerator enumerator = foundCollection.GetEnumerator();
					while (enumerator.MoveNext())
					{
						X509Certificate2 current = enumerator.Current;
						try
						{
							if (DecryptStringUsingCertificate(current, EncryptStringUsingCertificate(current, "test")) == "test")
							{
								privateCollection.Add(current);
							}
						}
						catch
						{
						}
						LongRunningActionForm.Instance.UpdateStatus(current.SimpleName());
					}
				});
			}
			finally
			{
				x509Store.Close();
			}
			X509Certificate2Collection x509Certificate2Collection = X509Certificate2UI.SelectFromCollection(privateCollection, "选择证书", "选择用于安全密码存储的证书", X509SelectionFlag.SingleSelection, Program.TheForm.Handle);
			return x509Certificate2Collection.Count != 1 ? null : x509Certificate2Collection[0];
		}

		public static X509Certificate2 GetCertificate(string thumbprint)
		{
			X509Store x509Store = new X509Store();
			x509Store.Open(OpenFlags.OpenExistingOnly);
			X509Certificate2Collection certificates = x509Store.Certificates;
			X509Certificate2Collection x509Certificate2Collection = certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);
			return x509Certificate2Collection.Count != 1 ? null : x509Certificate2Collection[0];
		}

		public static string EncryptString(string plaintext, EncryptionSettings settings)
		{
			switch (settings.EncryptionMethod.Value)
			{
			case EncryptionMethod.LogonCredentials:
				return EncryptStringUsingLocalUser(plaintext);
			case EncryptionMethod.Certificate:
			{
				X509Certificate2 certificate = GetCertificate(settings.CredentialData.Value);
				return EncryptStringUsingCertificate(certificate, plaintext);
			}
			default:
				throw new NotImplementedException("意外的加密方法“{0}”".InvariantFormat(settings.EncryptionMethod.Value.ToString()));
			}
		}

		private unsafe static string EncryptStringUsingLocalUser(string plaintext)
		{
			Crypto.DataBlob optionalEntropy = default(Crypto.DataBlob);
			Crypto.CryptProtectPromptStruct promptStruct = default(Crypto.CryptProtectPromptStruct);
			if (string.IsNullOrEmpty(plaintext))
			{
				return null;
			}
			optionalEntropy.Size = 0;
			promptStruct.Size = 0;
			char[] array = plaintext.ToCharArray();
			Crypto.DataBlob dataIn = default(Crypto.DataBlob);
			dataIn.Size = array.Length * 2;
			Crypto.DataBlob dataOut;
			fixed (char* ptr = array)
			{
				dataIn.Data = (IntPtr)ptr;
				if (!Crypto.CryptProtectData(ref dataIn, null, ref optionalEntropy, (IntPtr)(void*)null, ref promptStruct, 0, out dataOut))
				{
					FormTools.ErrorDialog("无法加密密码");
					return null;
				}
			}
			byte* ptr2 = (byte*)(void*)dataOut.Data;
			byte[] array2 = new byte[dataOut.Size];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = ptr2[i];
			}
			string result = Convert.ToBase64String(array2);
			Crypto.LocalFree(dataOut.Data);
			return result;
		}

		private static string EncryptStringUsingCertificate(X509Certificate2 cert, string plaintext)
		{
			RSACryptoServiceProvider rSACryptoServiceProvider = (RSACryptoServiceProvider)cert.PublicKey.Key;
			byte[] inArray = rSACryptoServiceProvider.Encrypt(Encoding.UTF8.GetBytes(plaintext), fOAEP: false);
			return Convert.ToBase64String(inArray);
		}

		public static string DecryptString(string encryptedString, EncryptionSettings settings)
		{
			if (string.IsNullOrEmpty(encryptedString))
			{
				return encryptedString;
			}
			switch (settings.EncryptionMethod.Value)
			{
			case EncryptionMethod.LogonCredentials:
				return DecryptStringUsingLocalUser(encryptedString);
			case EncryptionMethod.Certificate:
			{
				X509Certificate2 certificate = GetCertificate(settings.CredentialData.Value);
				if (certificate == null)
				{
					throw new Exception("未找到带有指纹“{1}”的证书“{0}”".InvariantFormat(settings.CredentialName.Value, settings.CredentialData.Value));
				}
				return DecryptStringUsingCertificate(certificate, encryptedString);
			}
			default:
				throw new NotImplementedException("意外的加密方法“{0}”".InvariantFormat(settings.EncryptionMethod.Value.ToString()));
			}
		}

		private unsafe static string DecryptStringUsingLocalUser(string encryptedString)
		{
			Crypto.DataBlob optionalEntropy = default(Crypto.DataBlob);
			Crypto.CryptProtectPromptStruct promptStruct = default(Crypto.CryptProtectPromptStruct);
			if (string.IsNullOrEmpty(encryptedString))
			{
				return string.Empty;
			}
			optionalEntropy.Size = 0;
			promptStruct.Size = 0;
			byte[] array = Convert.FromBase64String(encryptedString);
			Crypto.DataBlob dataIn = default(Crypto.DataBlob);
			dataIn.Size = array.Length;
			Crypto.DataBlob dataOut;
			string result;
			fixed (byte* ptr = array)
			{
				dataIn.Data = (IntPtr)ptr;
				if (!Crypto.CryptUnprotectData(ref dataIn, null, ref optionalEntropy, (IntPtr)(void*)null, ref promptStruct, 0, out dataOut))
				{
					throw new Exception("使用 {0} 凭证解密失败".InvariantFormat(CredentialsUI.GetLoggedInUser()));
				}
				char* ptr2 = (char*)(void*)dataOut.Data;
				char[] array2 = new char[dataOut.Size / 2];
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = ptr2[i];
				}
				result = new string(array2);
			}
			Crypto.LocalFree(dataOut.Data);
			return result;
		}

		private static string DecryptStringUsingCertificate(X509Certificate2 cert, string encryptedString)
		{
			if (string.IsNullOrEmpty(encryptedString))
			{
				return null;
			}
			RSACryptoServiceProvider rSACryptoServiceProvider = (RSACryptoServiceProvider)cert.PrivateKey;
			byte[] bytes = rSACryptoServiceProvider.Decrypt(Convert.FromBase64String(encryptedString), fOAEP: false);
			return Encoding.UTF8.GetString(bytes);
		}
	}
}

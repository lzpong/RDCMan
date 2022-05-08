using System;
using System.Collections.Generic;
using System.Xml;

namespace RdcMan {
	public class CredentialsStore {
		public const string XmlNodeName = "credentialsProfiles";

		public const string ProfileXmlNodeName = "credentialsProfile";

		private readonly Dictionary<string, CredentialsProfile> _profiles;

		public int ChangeId { get; private set; }

		public CredentialsProfile this[string name] {
			get => _profiles[name];
			set {
				_profiles[name] = value;
				ChangeId++;
			}
		}

		public IEnumerable<CredentialsProfile> Profiles => _profiles.Values;

		public CredentialsStore() {
			_profiles = new Dictionary<string, CredentialsProfile>(StringComparer.OrdinalIgnoreCase);
		}

		public void ReadXml(XmlNode xmlNode, ProfileScope scope, RdcTreeNode node, ICollection<string> errors) {
			foreach (XmlNode childNode in xmlNode.ChildNodes) {
				LogonCredentials logonCredentials = new LogonCredentials("", "credentialsProfile");
				logonCredentials.ReadXml(childNode, node, errors);
				ILogonCredentials logonCredentials2 = logonCredentials;
				CredentialsProfile credentialsProfile = new CredentialsProfile(logonCredentials2.ProfileName, scope, logonCredentials2.UserName, logonCredentials2.Password, logonCredentials2.Domain);
				this[logonCredentials2.ProfileName] = credentialsProfile;
				Encryption.DeferDecryption(credentialsProfile, node, credentialsProfile.QualifiedName);
			}
		}

		public void WriteXml(XmlTextWriter tw, RdcTreeNode node) {
			tw.WriteStartElement("credentialsProfiles");
			foreach (CredentialsProfile profile in Profiles) {
				LogonCredentials logonCredentials = new LogonCredentials("", "credentialsProfile");
				logonCredentials.InheritSettingsType.Mode = InheritanceMode.None;
				logonCredentials.ProfileName.Value = ((ILogonCredentials)profile).ProfileName;
				logonCredentials.UserName.Value = ((ILogonCredentials)profile).UserName;
				logonCredentials.Password.Copy(((ILogonCredentials)profile).Password);
				logonCredentials.Domain.Value = ((ILogonCredentials)profile).Domain;
				logonCredentials.WriteXml(tw, node);
			}
			tw.WriteEndElement();
		}

		public bool TryGetValue(string name, out CredentialsProfile profile) {
			return _profiles.TryGetValue(name, out profile);
		}

		public void Remove(string name) {
			_profiles.Remove(name);
			ChangeId++;
		}

		public bool Contains(string name) {
			return _profiles.ContainsKey(name);
		}
	}
}
